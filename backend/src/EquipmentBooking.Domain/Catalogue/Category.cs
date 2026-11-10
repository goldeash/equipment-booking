using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Catalogue;

/// <summary>Group of items sharing the maximum loan duration, usage guidelines and approval settings.</summary>
public sealed class Category : Entity
{
    private Category()
    {
        Name = string.Empty;
        UsageGuidelines = string.Empty;
    }

    public Category(string name, string? description, int maxLoanDays, string usageGuidelines, bool isSpecialised, bool autoApprovalEnabled)
    {
        Name = string.Empty;
        UsageGuidelines = string.Empty;
        Update(name, description, maxLoanDays, usageGuidelines, isSpecialised, autoApprovalEnabled);
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public int MaxLoanDays { get; private set; }

    public string UsageGuidelines { get; private set; }

    public bool IsSpecialised { get; private set; }

    public bool AutoApprovalEnabled { get; private set; }

    public void Update(string name, string? description, int maxLoanDays, string usageGuidelines, bool isSpecialised, bool autoApprovalEnabled)
    {
        if (maxLoanDays <= 0)
        {
            throw new DomainException(DomainErrorCodes.InvalidLoanDuration, "The maximum loan duration must be at least one day.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        MaxLoanDays = maxLoanDays;
        UsageGuidelines = usageGuidelines.Trim();
        IsSpecialised = isSpecialised;
        AutoApprovalEnabled = autoApprovalEnabled;
    }
}
