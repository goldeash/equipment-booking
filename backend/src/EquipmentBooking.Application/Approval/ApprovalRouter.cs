using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Application.Notifications;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Settings;

namespace EquipmentBooking.Application.Approval;

/// <summary>
/// Approval route of a request (US-24, US-25, US-26): fixed policy checks first, then the purpose
/// check by the assistant. A request is approved automatically only if both are positive; otherwise
/// it stays Requested and goes to the department manager with every reason. The assistant never declines.
/// </summary>
public sealed class ApprovalRouter
{
    private readonly IAppDbContext _db;
    private readonly ApprovalPolicyEvaluator _evaluator;
    private readonly IPurposeAssessor _purposeAssessor;
    private readonly NotificationService _notifications;
    private readonly TimeProvider _timeProvider;

    public ApprovalRouter(
        IAppDbContext db,
        ApprovalPolicyEvaluator evaluator,
        IPurposeAssessor purposeAssessor,
        NotificationService notifications,
        TimeProvider timeProvider)
    {
        _db = db;
        _evaluator = evaluator;
        _purposeAssessor = purposeAssessor;
        _notifications = notifications;
        _timeProvider = timeProvider;
    }

    /// <summary>Assesses a saved booking; the caller saves the changes.</summary>
    public async Task<ApprovalAssessment> RouteAsync(
        Booking booking,
        EquipmentItem item,
        Category category,
        OrganisationSettings settings,
        CancellationToken cancellationToken)
    {
        var evaluation = _evaluator.Evaluate(new ApprovalContext(item, category, settings, booking.Period));
        var assessment = new ApprovalAssessment(booking.Id, evaluation, _timeProvider.GetUtcNow());
        _db.ApprovalAssessments.Add(assessment);

        if (!assessment.IsPending)
        {
            return assessment;
        }

        // Only the purpose and the category data are sent; never the identity or history of the requester.
        var purpose = await _purposeAssessor.AssessAsync(
            new PurposeCheckRequest(booking.Purpose, category.Name, category.UsageGuidelines, item.Name),
            cancellationToken);
        assessment.CompletePurposeCheck(purpose);

        if (assessment.Outcome == AssessmentOutcomes.Approved && booking.RequesterId is { } requesterId)
        {
            booking.ApproveByAssistant(_timeProvider.GetUtcNow());
            _notifications.Notify(
                requesterId,
                NotificationTypeCodes.RequestApproved,
                booking.Id,
                "Request approved",
                $"Your request for {item.Name} ({booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}) was approved by the AI assistant.");
        }

        return assessment;
    }
}
