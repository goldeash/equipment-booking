using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Common.Paging;
using EquipmentBooking.Application.Common.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBooking.Application.Approval;

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
