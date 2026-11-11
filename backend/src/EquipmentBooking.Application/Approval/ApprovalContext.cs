using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Common;
using EquipmentBooking.Domain.Settings;

namespace EquipmentBooking.Application.Approval;

/// <summary>Facts about one request that the fixed policy checks evaluate.</summary>
public sealed record ApprovalContext(EquipmentItem Item, Category Category, OrganisationSettings Settings, DateRange Period);
