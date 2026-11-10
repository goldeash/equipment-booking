using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Domain.Bookings;
using FluentValidation;

namespace EquipmentBooking.Application.Bookings;

public sealed record CreateBookingRequest(long EquipmentItemId, DateOnly StartDate, DateOnly EndDate, string Purpose);

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.EquipmentItemId).GreaterThan(0);
        RuleFor(request => request.EndDate).GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("The end date must not be before the start date.");
        RuleFor(request => request.Purpose).NotEmpty()
            .Length(Booking.MinPurposeLength, Booking.MaxPurposeLength);
    }
}

public sealed record ChangeBookingDatesRequest(DateOnly StartDate, DateOnly EndDate);

public sealed class ChangeBookingDatesRequestValidator : AbstractValidator<ChangeBookingDatesRequest>
{
    public ChangeBookingDatesRequestValidator()
    {
        RuleFor(request => request.EndDate).GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage("The end date must not be before the start date.");
    }
}

public sealed record MyBookingsQuery : PageQuery
{
    /// <summary>Optional status filter, for example "approved".</summary>
    public string? Status { get; init; }
}

public sealed class MyBookingsQueryValidator : AbstractValidator<MyBookingsQuery>
{
    public MyBookingsQueryValidator()
    {
        RuleFor(query => query.Status).Must(status => status is null || BookingStatusCodes.All.Contains(status))
            .WithMessage($"'Status' must be one of: {string.Join(", ", BookingStatusCodes.All)}.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}

public sealed record BookingItemInfo(long Id, string InventoryNumber, string Name, string CategoryName);

/// <summary>Who took the current decision: a manager (with name) or the AI assistant.</summary>
public sealed record DecisionInfo(string Source, string Outcome, string? DecidedByName, string? Comment, DateTimeOffset DecidedAt);

public sealed record BookingSummary(
    long Id,
    BookingItemInfo Item,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    DecisionInfo? Decision,
    DateTimeOffset CreatedAt);

public sealed record LoanInfo(
    DateTimeOffset IssuedAt,
    DateTimeOffset DueAt,
    DateTimeOffset? ReturnedAt,
    string? ReturnConditionCode,
    string? ReturnComment);

public sealed record RoutingInfo(string Outcome, string PurposeResult, IReadOnlyList<string> Reasons, string? Summary, DateTimeOffset AssessedAt);

public sealed record BookingDetails(
    long Id,
    BookingItemInfo Item,
    long? RequesterId,
    string? RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Purpose,
    string Status,
    DecisionInfo? Decision,
    LoanInfo? Loan,
    RoutingInfo? Routing,
    DateTimeOffset CreatedAt,
    bool CanCancel,
    bool CanChangeDates);
