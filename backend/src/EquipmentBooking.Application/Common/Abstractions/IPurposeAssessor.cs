using EquipmentBooking.Domain.Approval;

namespace EquipmentBooking.Application.Common.Abstractions;

/// <summary>Checks whether a stated purpose matches the usage guidelines of the category.</summary>
public interface IPurposeAssessor
{
    Task<PurposeAssessment> AssessAsync(PurposeCheckRequest request, CancellationToken cancellationToken);
}
