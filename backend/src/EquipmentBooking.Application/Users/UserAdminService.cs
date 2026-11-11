using System.Linq.Expressions;
using EquipmentBooking.Application.Auth;
using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Users;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Users;

/// <summary>User accounts, departments and roles managed by administrators (US-34).</summary>
public sealed class UserAdminService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHashing _passwordHashing;
    private readonly AuthService _authService;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<UsersQuery> _queryValidator;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly IValidator<SetPasswordRequest> _passwordValidator;

    public UserAdminService(
        IAppDbContext db,
        ICurrentUser currentUser,
        IPasswordHashing passwordHashing,
        AuthService authService,
        TimeProvider timeProvider,
        IValidator<UsersQuery> queryValidator,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        IValidator<SetPasswordRequest> passwordValidator)
    {
        _db = db;
        _currentUser = currentUser;
        _passwordHashing = passwordHashing;
        _authService = authService;
        _timeProvider = timeProvider;
        _queryValidator = queryValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _passwordValidator = passwordValidator;
    }

    public async Task<PagedResult<UserResponse>> GetAsync(UsersQuery query, CancellationToken cancellationToken)
    {
        await _queryValidator.EnsureValidAsync(query, cancellationToken);
        var users = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{query.Q.Trim()}%";
            users = users.Where(user => EF.Functions.Like(user.FullName.ToLower(), pattern.ToLower()) || EF.Functions.Like(user.Email.ToLower(), pattern.ToLower()));
        }

        if (query.Role is not null)
        {
            users = users.Where(user => user.RoleCode == query.Role);
        }

        if (query.DepartmentId is { } departmentId)
        {
            users = users.Where(user => user.DepartmentId == departmentId);
        }

        return await users.OrderBy(user => user.FullName).ThenBy(user => user.Id)
            .Select(Projection)
            .ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);
        await EnsureDepartmentExistsAsync(request.DepartmentId, cancellationToken);
        var user = new User(request.Email, request.FullName, request.Role, request.DepartmentId);
        user.SetPasswordHash(_passwordHashing.Hash(user, request.Password));
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    /// <summary>Updates a user; a role change or deactivation ends all sessions, so new permissions apply at the next sign in.</summary>
    public async Task<UserResponse> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);
        await EnsureDepartmentExistsAsync(request.DepartmentId, cancellationToken);
        var user = await _db.Users.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw new NotFoundException("User", id);
        if (id == _currentUser.UserId && (!request.IsActive || request.Role != RoleCodes.Administrator))
        {
            throw new BusinessRuleException(ErrorCodes.Forbidden, "Administrators cannot deactivate themselves or remove their own administrator role.");
        }

        var roleChanged = user.ChangeRole(request.Role);
        var departmentChanged = user.DepartmentId != request.DepartmentId;
        user.UpdateProfile(request.FullName, request.DepartmentId);
        var deactivated = user.IsActive && !request.IsActive;
        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        if (roleChanged || departmentChanged || deactivated)
        {
            await _authService.RevokeAllAsync(user.Id, _timeProvider.GetUtcNow(), cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task SetPasswordAsync(long id, SetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _passwordValidator.EnsureValidAsync(request, cancellationToken);
        var user = await _db.Users.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)
            ?? throw new NotFoundException("User", id);
        user.SetPasswordHash(_passwordHashing.Hash(user, request.Password));
        user.RegisterSuccessfulSignIn();
        await _authService.RevokeAllAsync(user.Id, _timeProvider.GetUtcNow(), cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserResponse> GetByIdAsync(long id, CancellationToken cancellationToken) =>
        await _db.Users.AsNoTracking().Where(user => user.Id == id).Select(Projection).SingleAsync(cancellationToken);

    private async Task EnsureDepartmentExistsAsync(long departmentId, CancellationToken cancellationToken)
    {
        if (!await _db.Departments.AnyAsync(department => department.Id == departmentId, cancellationToken))
        {
            throw new ValidationException([new ValidationFailure("DepartmentId", "The department does not exist.")]);
        }
    }

    private static readonly Expression<Func<User, UserResponse>> Projection = user => new UserResponse(
        user.Id,
        user.Email,
        user.FullName,
        user.RoleCode,
        user.DepartmentId,
        user.Department!.Name,
        user.IsActive,
        user.CreatedAt);
}
