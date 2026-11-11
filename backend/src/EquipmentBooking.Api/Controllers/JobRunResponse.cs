namespace EquipmentBooking.Api.Controllers;

public sealed record JobRunResponse(string Job, int? Processed, bool AlreadyRunning);
