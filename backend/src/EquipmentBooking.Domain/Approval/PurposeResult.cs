using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Approval;

public sealed class PurposeResult : ReferenceData
{
    private PurposeResult()
    {
    }

    public PurposeResult(string code, string name)
        : base(code, name)
    {
    }
}
