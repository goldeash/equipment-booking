using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Users;

/// <summary>Person who signs in to the system with one role and one department.</summary>
public sealed class User : AuditableEntity
{
    public const int MaxFailedSignIns = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private User()
    {
        Email = string.Empty;
        FullName = string.Empty;
        PasswordHash = string.Empty;
        RoleCode = string.Empty;
    }

    public User(string email, string fullName, string roleCode, long departmentId)
    {
        Email = email.Trim();
        FullName = fullName.Trim();
        RoleCode = roleCode;
        DepartmentId = departmentId;
        PasswordHash = string.Empty;
        IsActive = true;
    }

    public string Email { get; private set; }

    public string FullName { get; private set; }

    public string PasswordHash { get; private set; }

    public string RoleCode { get; private set; }

    public long DepartmentId { get; private set; }

    public Department? Department { get; private set; }

    public bool IsActive { get; private set; }

    public int FailedSignInCount { get; private set; }

    public DateTimeOffset? LockoutEndsAt { get; private set; }

    public DateTimeOffset? PrivacyNoticeAcknowledgedAt { get; private set; }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEndsAt is { } end && end > now;

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void UpdateProfile(string fullName, long departmentId)
    {
        FullName = fullName.Trim();
        DepartmentId = departmentId;
    }

    /// <summary>Changes the role and reports whether it actually changed.</summary>
    public bool ChangeRole(string roleCode)
    {
        if (RoleCode == roleCode)
        {
            return false;
        }

        RoleCode = roleCode;
        return true;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void RegisterFailedSignIn(DateTimeOffset now)
    {
        FailedSignInCount++;
        if (FailedSignInCount >= MaxFailedSignIns)
        {
            LockoutEndsAt = now.Add(LockoutDuration);
            FailedSignInCount = 0;
        }
    }

    public void RegisterSuccessfulSignIn()
    {
        FailedSignInCount = 0;
        LockoutEndsAt = null;
    }

    public void AcknowledgePrivacyNotice(DateTimeOffset now) => PrivacyNoticeAcknowledgedAt ??= now;
}
