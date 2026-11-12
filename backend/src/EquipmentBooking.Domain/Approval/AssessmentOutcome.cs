using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Approval;

public sealed class AssessmentOutcome : ReferenceData
{
    private AssessmentOutcome()
    {
    }

    public AssessmentOutcome(string code, string name)
        : base(code, name)
    {
    }
}
