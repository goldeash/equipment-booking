using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Validation;
using EquipmentBooking.Domain.Approval;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Approval;

public sealed record AssessmentLogQuery : PageQuery
{
    public string? Outcome { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}

public sealed class AssessmentLogQueryValidator : AbstractValidator<AssessmentLogQuery>
{
    public AssessmentLogQueryValidator()
    {
        RuleFor(query => query.Outcome)
            .Must(outcome => outcome is null or AssessmentOutcomes.Pending or AssessmentOutcomes.Approved or AssessmentOutcomes.Routed)
            .WithMessage("'Outcome' must be pending, approved or routed.");
        RuleFor(query => query.To).GreaterThanOrEqualTo(query => query.From).When(query => query.From.HasValue && query.To.HasValue);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PageQuery.MaxPageSize);
    }
}

public sealed record AssessmentLogEntry(
    long Id,
    long BookingId,
    string ItemName,
    string InventoryNumber,
    DateTimeOffset AssessedAt,
    bool ValueCheckPassed,
    bool SpecialisedCheckPassed,
    bool DurationCheckPassed,
    bool OrganisationAutoApprovalEnabled,
    bool CategoryAutoApprovalEnabled,
    string PurposeResult,
    string Outcome,
    IReadOnlyList<string> Reasons);

/// <summary>Decision log of the approval route for administrators (US-26).</summary>
public sealed class AssessmentLogQueries
{
    private readonly IAppDbContext _db;
    private readonly IValidator<AssessmentLogQuery> _validator;

    public AssessmentLogQueries(IAppDbContext db, IValidator<AssessmentLogQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<PagedResult<AssessmentLogEntry>> GetAsync(AssessmentLogQuery query, CancellationToken cancellationToken)
    {
        await _validator.EnsureValidAsync(query, cancellationToken);
        var assessments = _db.ApprovalAssessments.AsNoTracking();
        if (query.Outcome is not null)
        {
            assessments = assessments.Where(assessment => assessment.Outcome == query.Outcome);
        }

        if (query.From is { } from)
        {
            var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            assessments = assessments.Where(assessment => assessment.AssessedAt >= start);
        }

        if (query.To is { } to)
        {
            var end = new DateTimeOffset(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            assessments = assessments.Where(assessment => assessment.AssessedAt < end);
        }

        return await assessments
            .OrderByDescending(assessment => assessment.AssessedAt)
            .ThenByDescending(assessment => assessment.Id)
            .Join(_db.Bookings, assessment => assessment.BookingId, booking => booking.Id, (assessment, booking) => new { assessment, booking })
            .Select(row => new AssessmentLogEntry(
                row.assessment.Id,
                row.booking.Id,
                row.booking.EquipmentItem!.Name,
                row.booking.EquipmentItem.InventoryNumber,
                row.assessment.AssessedAt,
                row.assessment.ValueCheckPassed,
                row.assessment.SpecialisedCheckPassed,
                row.assessment.DurationCheckPassed,
                row.assessment.OrganisationAutoApprovalEnabled,
                row.assessment.CategoryAutoApprovalEnabled,
                row.assessment.PurposeResult,
                row.assessment.Outcome,
                row.assessment.Reasons.Select(reason => reason.RoutingReasonCode).ToList()))
            .ToPagedResultAsync(query, cancellationToken);
    }
}
