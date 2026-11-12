namespace EquipmentBooking.Domain.Approval;

/// <summary>Codes of the fixed policy checks; every assessment stores one result for each of them.</summary>
public static class ApprovalCheckTypeCodes
{
    public const string OrganisationAutoApproval = "organisation_auto_approval";
    public const string CategoryAutoApproval = "category_auto_approval";
    public const string ValueThreshold = "value_threshold";
    public const string NotSpecialised = "not_specialised";
    public const string MaxDuration = "max_duration";

    public static readonly IReadOnlyList<string> All = [OrganisationAutoApproval, CategoryAutoApproval, ValueThreshold, NotSpecialised, MaxDuration];
}
