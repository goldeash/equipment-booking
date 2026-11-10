namespace EquipmentBooking.Domain.Common;

/// <summary>Fixed reference value identified by a readable code.</summary>
public abstract class ReferenceData
{
    protected ReferenceData()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    protected ReferenceData(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }
}
