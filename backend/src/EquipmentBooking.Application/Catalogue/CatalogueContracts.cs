using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Domain.Catalogue;
using FluentValidation;

namespace EquipmentBooking.Application.Catalogue;

/// <summary>Filters of the catalogue search (US-03).</summary>
public sealed record EquipmentListQuery : PageQuery
{
    public string? Q { get; init; }

    public long? CategoryId { get; init; }

    public long? LocationId { get; init; }

    public long[]? FeatureIds { get; init; }

    /// <summary>With <see cref="AvailableTo"/>: only items free for the whole inclusive range.</summary>
    public DateOnly? AvailableFrom { get; init; }

    public DateOnly? AvailableTo { get; init; }

    /// <summary>Administrators may include retired items.</summary>
    public bool IncludeRetired { get; init; }

    /// <summary>name, inventoryNumber or value; prefix with "-" for descending order.</summary>
    public string? Sort { get; init; }
}

public sealed class EquipmentListQueryValidator : AbstractValidator<EquipmentListQuery>
{
    public static readonly IReadOnlyList<string> SortFields = ["name", "-name", "inventoryNumber", "-inventoryNumber", "value", "-value"];

    public EquipmentListQueryValidator()
    {
        RuleFor(query => query.Q).MaximumLength(100);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
        RuleFor(query => query.Sort).Must(sort => sort is null || SortFields.Contains(sort))
            .WithMessage($"'Sort' must be one of: {string.Join(", ", SortFields)}.");
        RuleFor(query => query)
            .Must(query => query.AvailableFrom.HasValue == query.AvailableTo.HasValue)
            .WithName("availableTo")
            .WithMessage("Both 'availableFrom' and 'availableTo' must be given.");
        RuleFor(query => query.AvailableTo)
            .GreaterThanOrEqualTo(query => query.AvailableFrom)
            .When(query => query.AvailableFrom.HasValue && query.AvailableTo.HasValue)
            .WithMessage("'availableTo' must not be before 'availableFrom'.");
    }
}

public sealed record EquipmentListItem(
    long Id,
    string InventoryNumber,
    string Name,
    long CategoryId,
    string CategoryName,
    long LocationId,
    string LocationName,
    decimal Value,
    string ConditionCode,
    string Status,
    bool IsSpecialised,
    IReadOnlyList<string> Features);

public sealed record CategorySummary(long Id, string Name, int MaxLoanDays, string UsageGuidelines, bool IsSpecialised);

public sealed record NamedItem(long Id, string Name);

public sealed record EquipmentDetails(
    long Id,
    string InventoryNumber,
    string Name,
    string? Description,
    CategorySummary Category,
    NamedItem Location,
    decimal Value,
    string ConditionCode,
    string Status,
    bool IsSpecialised,
    IReadOnlyList<NamedItem> Features,
    DateTimeOffset? RetiredAt);

/// <summary>Period in which the item is not free; never contains the names of other employees (US-04).</summary>
public sealed record BookedPeriod(DateOnly StartDate, DateOnly? EndDate, string Kind)
{
    public const string BookingKind = "booking";
    public const string MaintenanceKind = "maintenance";
}

public sealed record BookedPeriodsQuery(DateOnly From, DateOnly To);

public sealed class BookedPeriodsQueryValidator : AbstractValidator<BookedPeriodsQuery>
{
    public const int MaxRangeDays = 366;

    public BookedPeriodsQueryValidator()
    {
        RuleFor(query => query.To).GreaterThanOrEqualTo(query => query.From);
        RuleFor(query => query)
            .Must(query => query.To.DayNumber - query.From.DayNumber <= MaxRangeDays)
            .WithName("to")
            .WithMessage($"The range must not exceed {MaxRangeDays} days.");
    }
}

/// <summary>Event in the history of an item (US-32).</summary>
public sealed record EquipmentHistoryEvent(DateTimeOffset OccurredAt, string Kind, string Summary, long? BookingId);

public sealed record SaveEquipmentRequest(
    string InventoryNumber,
    string Name,
    string? Description,
    long CategoryId,
    long LocationId,
    decimal Value,
    string ConditionCode,
    bool IsSpecialised,
    long[]? FeatureIds);

public sealed class SaveEquipmentRequestValidator : AbstractValidator<SaveEquipmentRequest>
{
    public SaveEquipmentRequestValidator()
    {
        RuleFor(request => request.InventoryNumber).NotEmpty().MaximumLength(30).Matches("^[A-Za-z0-9-]+$")
            .WithMessage("'Inventory Number' may contain letters, digits and hyphens only.");
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Description).MaximumLength(2000);
        RuleFor(request => request.CategoryId).GreaterThan(0);
        RuleFor(request => request.LocationId).GreaterThan(0);
        RuleFor(request => request.Value).InclusiveBetween(0m, 9_999_999_999.99m).PrecisionScale(12, 2, ignoreTrailingZeros: true);
        RuleFor(request => request.ConditionCode).Must(code => ItemConditionCodes.All.Contains(code))
            .WithMessage($"'Condition Code' must be one of: {string.Join(", ", ItemConditionCodes.All)}.");
        RuleFor(request => request.FeatureIds).Must(ids => ids is null || ids.Length <= 20)
            .WithMessage("At most 20 features can be assigned.");
    }
}

public sealed record CategoryResponse(
    long Id,
    string Name,
    string? Description,
    int MaxLoanDays,
    string UsageGuidelines,
    bool IsSpecialised,
    bool AutoApprovalEnabled);

public sealed record SaveCategoryRequest(
    string Name,
    string? Description,
    int MaxLoanDays,
    string UsageGuidelines,
    bool IsSpecialised,
    bool AutoApprovalEnabled);

public sealed class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.MaxLoanDays).InclusiveBetween(1, 365);
        RuleFor(request => request.UsageGuidelines).NotEmpty().MaximumLength(2000);
    }
}

public sealed record CreateNamedItemRequest(string Name);

public sealed class CreateNamedItemRequestValidator : AbstractValidator<CreateNamedItemRequest>
{
    public CreateNamedItemRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed record CodeNameItem(string Code, string Name);
