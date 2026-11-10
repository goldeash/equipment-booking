using EquipmentBooking.Application.Common.Abstractions;
using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Infrastructure.Ai;

/// <summary>
/// Purpose assessor used while no language model is configured. It reports the assistant as
/// unavailable, so every request that passes the policy checks is routed to the department
/// manager (NFR-07). This is the documented fallback behaviour, not a simulated AI decision.
/// </summary>
public sealed class UnavailablePurposeAssessor : IPurposeAssessor
{
    public Task<PurposeAssessment> AssessAsync(PurposeCheckRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(PurposeAssessment.Unavailable);
}
