using Bogus;
using EquipmentBooking.Domain.Approval;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Catalogue;
using EquipmentBooking.Domain.Maintenance;
using EquipmentBooking.Domain.Notifications;
using EquipmentBooking.Domain.Users;

namespace EquipmentBooking.DbSeeder;

/// <summary>
/// Generates a deterministic, realistic demo data set relative to a reference date: six months of
/// loan history, current and overdue loans, upcoming requests and maintenance. All people are fictional.
/// </summary>
internal sealed class DemoDataGenerator
{
    public const string EmailDomain = "company.example";
    public const string DemoAdministratorEmail = "admin@" + EmailDomain;
    public const string DemoManagerEmail = "manager@" + EmailDomain;
    public const string DemoEmployeeEmail = "employee@" + EmailDomain;

    private const int RandomSeed = 20261110;
    private const int HistoryDays = 180;
    private const int FutureDays = 40;
    private const decimal LowValueThreshold = 500m;

    private static readonly TimeOnly ReturnTime = new(18, 0);

    private static readonly string[] Purposes =
    [
        "Client presentation at the Kaunas office",
        "Recording an interview with a customer for the case study",
        "Product demonstration at a partner meeting",
        "Training session for new employees",
        "Site visit to document the installation",
        "Temporary replacement while my device is repaired",
        "Quarterly planning workshop with the team",
        "Webinar for customers about the new release",
        "Testing the mobile application before release",
        "Trade fair stand in Riga",
        "Internal hackathon in the main office",
        "Measurements at the customer site",
        "Business trip to Warsaw",
        "Video recording of the onboarding course",
        "Remote meeting with the supplier",
    ];

    private static readonly string[] DeclineComments =
    [
        "Please use the meeting room equipment for this purpose.",
        "The item is reserved for the trade fair team this week.",
        "A cheaper item from the same category is enough for this task.",
        "Please agree the trip with your team lead first.",
    ];

    private static readonly string[] DamageComments =
    [
        "Cracked screen corner.",
        "Lens cap missing and scratches on the lens.",
        "Battery does not hold charge.",
        "Broken charging port.",
        "Tripod thread damaged.",
    ];

    private readonly DateOnly _today;
    private readonly DateTimeOffset _now;
    private readonly TimeZoneInfo _zone;
    private readonly Randomizer _random;
    private readonly Faker _faker;
    private readonly DemoDataSet _data = new();
    private long _nextBookingId = 1;
    private long _nextDecisionId = 1;
    private long _nextAssessmentId = 1;
    private long _nextMaintenanceId = 1;

    public DemoDataGenerator(DateTimeOffset now, string timeZoneId)
    {
        _zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        _now = now;
        _today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, _zone).DateTime);
        _random = new Randomizer(RandomSeed);
        _faker = new Faker("en") { Random = new Randomizer(RandomSeed) };
    }

    public DemoDataSet Generate()
    {
        CreateReferenceData();
        CreateUsers();
        CreateItems();
        foreach (var item in _data.Items)
        {
            CreateTimeline(item);
        }

        return _data;
    }

    private void CreateReferenceData()
    {
        _data.Departments.AddRange(DemoCatalogue.Departments.Select((name, index) => new DepartmentRow(index + 1, name)));
        _data.Locations.AddRange(DemoCatalogue.Locations.Select((name, index) => new LocationRow(index + 1, name)));
        _data.Features.AddRange(DemoCatalogue.Features.Select((name, index) => new FeatureRow(index + 1, name)));
        _data.Categories.AddRange(DemoCatalogue.Categories.Select((spec, index) => new CategoryRow(index + 1, spec)));
    }

    private void CreateUsers()
    {
        var created = At(_today.AddDays(-HistoryDays - 30), new TimeOnly(9, 0));
        var usedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long id = 1;

        void Add(string email, string fullName, string role, long departmentId)
        {
            usedEmails.Add(email);
            _data.Users.Add(new UserRow(id++, email, fullName, role, departmentId, created));
        }

        void AddRandom(string role, long departmentId)
        {
            var (fullName, email) = NextPerson(usedEmails);
            Add(email, fullName, role, departmentId);
        }

        Add(DemoAdministratorEmail, "Laura Bennett", RoleCodes.Administrator, DepartmentId("Operations"));
        AddRandom(RoleCodes.Administrator, DepartmentId("IT Support"));
        Add(DemoManagerEmail, "Daniel Hughes", RoleCodes.Manager, DepartmentId("Engineering"));
        foreach (var department in _data.Departments.Where(department => department.Name != "Engineering"))
        {
            AddRandom(RoleCodes.Manager, department.Id);
        }

        Add(DemoEmployeeEmail, "Emma Clarke", RoleCodes.Employee, DepartmentId("Engineering"));
        for (var index = 0; index < 49; index++)
        {
            var department = _data.Departments[index % _data.Departments.Count];
            AddRandom(RoleCodes.Employee, department.Id);
        }
    }

    private void CreateItems()
    {
        var created = At(_today.AddDays(-HistoryDays - 20), new TimeOnly(10, 0));
        long id = 1;
        foreach (var category in _data.Categories)
        {
            var prefix = new string([.. category.Spec.Name.Where(char.IsLetter).Take(3)]).ToUpperInvariant();
            for (var index = 0; index < category.Spec.ItemCount; index++)
            {
                var model = category.Spec.Models[index % category.Spec.Models.Length];
                var condition = _random.WeightedRandom([ItemConditionCodes.New, ItemConditionCodes.Good, ItemConditionCodes.Fair], [0.25f, 0.6f, 0.15f]);
                _data.Items.Add(new ItemRow
                {
                    Id = id,
                    InventoryNumber = $"EQ-{prefix}-{index + 1:000}",
                    Name = model.Name,
                    Description = $"{model.Name} from the {category.Spec.Name.ToLowerInvariant()} pool. {string.Join(", ", model.Features)}.",
                    Category = category,
                    LocationId = _data.Locations[(int)(id % _data.Locations.Count)].Id,
                    Value = model.Value,
                    InitialCondition = condition,
                    Condition = condition,
                    FeatureIds = [.. model.Features.Select(FeatureId)],
                    CreatedAt = created,
                });
                _data.ConditionChanges.Add(new ConditionChangeRow(id, null, condition, created, AdministratorId));
                id++;
            }
        }
    }

    private void CreateTimeline(ItemRow item)
    {
        var day = _today.AddDays(-HistoryDays + _random.Int(0, 10));
        var end = _today.AddDays(FutureDays);
        var holdsOverdue = _random.Bool(0.05f);
        var currentRepair = !holdsOverdue && _random.Bool(0.03f);

        while (day < end)
        {
            if (currentRepair && day >= _today.AddDays(-3))
            {
                AddMaintenance(item, MaintenanceTypeCodes.Repair, _today.AddDays(-1), null, "Device does not start; sent to the service partner.");
                return;
            }

            if (_random.Bool(0.05f))
            {
                var length = _random.Int(1, 3);
                var type = item.Category.Spec.IsSpecialised ? MaintenanceTypeCodes.Calibration : MaintenanceTypeCodes.Inspection;
                AddMaintenance(item, type, day, day.AddDays(length - 1), type == MaintenanceTypeCodes.Calibration ? "Scheduled calibration." : "Scheduled inspection and cleaning.");
                day = day.AddDays(length + _random.Int(1, 4));
                continue;
            }

            var duration = _random.Int(1, Math.Min(item.Category.Spec.MaxLoanDays, 7));
            var startDate = day;
            var endDate = day.AddDays(duration - 1);
            var next = AddBooking(item, startDate, endDate, holdsOverdue);
            if (next is null)
            {
                return;
            }

            day = next.Value.AddDays(_random.Int(1, 9));
        }
    }

    /// <summary>Adds a booking and returns the last blocked day, or null when the item stays blocked.</summary>
    private DateOnly? AddBooking(ItemRow item, DateOnly startDate, DateOnly endDate, bool holdsOverdue)
    {
        var requester = _random.ArrayElement(_data.Users.Where(user => user.Role == RoleCodes.Employee).ToArray());
        var manager = ManagerOf(requester);
        var createdAt = NotLaterThanNow(At(startDate.AddDays(-_random.Int(2, 12)), new TimeOnly(_random.Int(8, 16), _random.Int(0, 59))), TimeSpan.FromHours(2));
        var booking = new BookingRow
        {
            Id = _nextBookingId++,
            Item = item,
            Requester = requester,
            StartDate = startDate,
            EndDate = endDate,
            Purpose = _random.ArrayElement(Purposes),
            Status = BookingStatusCodes.Requested,
            CreatedAt = createdAt,
        };
        _data.Bookings.Add(booking);
        _data.StatusChanges.Add(new StatusChangeRow(booking.Id, null, BookingStatusCodes.Requested, createdAt, requester.Id));
        AddAssessment(booking, createdAt.AddSeconds(1));

        if (startDate > _today)
        {
            if (_random.Bool(0.55f))
            {
                Decide(booking, manager, DecisionOutcomes.Approved, null, NotLaterThanNow(createdAt.AddHours(_random.Int(1, 30)), TimeSpan.FromMinutes(10)));
            }

            return endDate;
        }

        var decidedAt = NotLaterThanNow(Min(createdAt.AddHours(_random.Int(1, 30)), At(startDate, new TimeOnly(8, 30))), TimeSpan.FromMinutes(30));
        if (endDate < _today && _random.Bool(0.08f))
        {
            Decide(booking, manager, DecisionOutcomes.Declined, _random.ArrayElement(DeclineComments), decidedAt);
            return endDate;
        }

        Decide(booking, manager, DecisionOutcomes.Approved, null, decidedAt);
        if (endDate < _today && _random.Bool(0.07f))
        {
            Change(booking, BookingStatusCodes.Cancelled, decidedAt.AddHours(_random.Int(1, 20)), requester.Id);
            return endDate;
        }

        var issuedAt = NotLaterThanNow(At(startDate, new TimeOnly(_random.Int(8, 11), _random.Int(0, 59))), TimeSpan.FromMinutes(5));
        if (issuedAt <= decidedAt)
        {
            issuedAt = decidedAt.AddMinutes(1);
        }

        var dueAt = At(endDate, ReturnTime);
        Change(booking, BookingStatusCodes.Issued, issuedAt, AdministratorId);

        if (endDate >= _today)
        {
            _data.Loans.Add(new LoanRow(booking.Id, issuedAt, AdministratorId, dueAt, null, null, null, null));
            return endDate;
        }

        var daysSinceDue = _today.DayNumber - endDate.DayNumber;
        if (holdsOverdue && daysSinceDue <= 6)
        {
            _data.Loans.Add(new LoanRow(booking.Id, issuedAt, AdministratorId, dueAt, null, null, null, null));
            Change(booking, BookingStatusCodes.Overdue, dueAt.AddMinutes(5), null);
            _data.Notifications.Add(new NotificationRow(
                requester.Id,
                NotificationTypeCodes.LoanOverdue,
                booking.Id,
                "Loan overdue",
                $"{item.Name} ({item.InventoryNumber}) was due on {endDate:yyyy-MM-dd}. Please return it as soon as possible.",
                dueAt.AddMinutes(5),
                null));
            return null;
        }

        var late = _random.Bool(0.15f) && dueAt.AddHours(70) < _now;
        var returnedAt = late
            ? dueAt.AddHours(_random.Int(18, 70))
            : Max(At(endDate, new TimeOnly(_random.Int(9, 17), _random.Int(0, 59))), issuedAt.AddHours(1));
        var damaged = _random.Bool(0.05f);
        var condition = damaged ? ItemConditionCodes.Damaged : item.Condition;
        var comment = damaged ? _random.ArrayElement(DamageComments) : null;
        _data.Loans.Add(new LoanRow(booking.Id, issuedAt, AdministratorId, dueAt, returnedAt, AdministratorId, condition, comment));
        Change(booking, BookingStatusCodes.Returned, returnedAt, AdministratorId);

        if (!damaged)
        {
            return late ? DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(returnedAt, _zone).DateTime) : endDate;
        }

        var previous = item.Condition;
        item.Condition = ItemConditionCodes.Damaged;
        _data.ConditionChanges.Add(new ConditionChangeRow(item.Id, previous, ItemConditionCodes.Damaged, returnedAt, AdministratorId));

        var repairStart = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(returnedAt, _zone).DateTime).AddDays(1);
        var repairEnd = repairStart.AddDays(_random.Int(2, 5));
        AddMaintenance(item, MaintenanceTypeCodes.Repair, repairStart, repairEnd < _today ? repairEnd : null, $"Repair after return: {comment}");
        if (repairEnd >= _today)
        {
            return null;
        }

        item.Condition = ItemConditionCodes.Good;
        _data.ConditionChanges.Add(new ConditionChangeRow(item.Id, ItemConditionCodes.Damaged, ItemConditionCodes.Good, At(repairEnd, new TimeOnly(15, 0)), AdministratorId));
        return repairEnd;
    }

    private void AddAssessment(BookingRow booking, DateTimeOffset assessedAt)
    {
        var category = booking.Item.Category.Spec;
        var valuePassed = booking.Item.Value < LowValueThreshold;
        var specialisedPassed = !category.IsSpecialised;
        var durationPassed = booking.EndDate.DayNumber - booking.StartDate.DayNumber + 1 <= category.MaxLoanDays;
        var reasons = new List<string>();
        if (!valuePassed)
        {
            reasons.Add(RoutingReasonCodes.HighValue);
        }

        if (!specialisedPassed)
        {
            reasons.Add(RoutingReasonCodes.Specialised);
        }

        if (!durationPassed)
        {
            reasons.Add(RoutingReasonCodes.DurationExceeded);
        }

        var purposeResult = reasons.Count > 0 ? PurposeResults.NotRun : PurposeResults.Unavailable;
        if (reasons.Count == 0)
        {
            reasons.Add(RoutingReasonCodes.AssistantUnavailable);
        }

        _data.Assessments.Add(new AssessmentRow(_nextAssessmentId++, booking.Id, assessedAt, valuePassed, specialisedPassed, durationPassed, purposeResult, reasons));
    }

    private void Decide(BookingRow booking, UserRow manager, string outcome, string? comment, DateTimeOffset decidedAt)
    {
        _data.Decisions.Add(new DecisionRow(_nextDecisionId++, booking.Id, manager.Id, outcome, comment, decidedAt));
        var status = outcome == DecisionOutcomes.Approved ? BookingStatusCodes.Approved : BookingStatusCodes.Declined;
        Change(booking, status, decidedAt, manager.Id);
        if (decidedAt >= At(_today.AddDays(-30), TimeOnly.MinValue))
        {
            var approved = outcome == DecisionOutcomes.Approved;
            _data.Notifications.Add(new NotificationRow(
                booking.Requester.Id,
                approved ? NotificationTypeCodes.RequestApproved : NotificationTypeCodes.RequestDeclined,
                booking.Id,
                approved ? "Request approved" : "Request declined",
                $"Your request for {booking.Item.Name} ({booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}) was {(approved ? "approved" : "declined")} by {manager.FullName}.{(comment is null ? string.Empty : $" Comment: {comment}")}",
                decidedAt,
                decidedAt < At(_today.AddDays(-3), TimeOnly.MinValue) ? decidedAt.AddHours(2) : null));
        }
    }

    private void Change(BookingRow booking, string status, DateTimeOffset at, long? userId)
    {
        _data.StatusChanges.Add(new StatusChangeRow(booking.Id, booking.Status, status, at, userId));
        booking.Status = status;
    }

    private void AddMaintenance(ItemRow item, string type, DateOnly start, DateOnly? end, string description) =>
        _data.Maintenance.Add(new MaintenanceRow(
            _nextMaintenanceId++,
            item.Id,
            type,
            start,
            end,
            type == MaintenanceTypeCodes.Repair || type == MaintenanceTypeCodes.Calibration,
            description,
            AdministratorId,
            At(start, new TimeOnly(9, 0))));

    private long AdministratorId => _data.Users[0].Id;

    private UserRow ManagerOf(UserRow user) =>
        _data.Users.First(candidate => candidate.Role == RoleCodes.Manager && candidate.DepartmentId == user.DepartmentId);

    private long DepartmentId(string name) => _data.Departments.Single(department => department.Name == name).Id;

    private long FeatureId(string name) => _data.Features.Single(feature => feature.Name == name).Id;

    private (string FullName, string Email) NextPerson(HashSet<string> used)
    {
        while (true)
        {
            var first = _faker.Name.FirstName();
            var last = _faker.Name.LastName();
            var email = $"{first}.{last}@{EmailDomain}".ToLowerInvariant().Replace("'", string.Empty, StringComparison.Ordinal);
            if (used.Add(email))
            {
                return ($"{first} {last}", email);
            }
        }
    }

    /// <summary>Keeps generated events in the past relative to the generation time.</summary>
    private DateTimeOffset NotLaterThanNow(DateTimeOffset value, TimeSpan margin) =>
        Min(value, _now - margin);

    private DateTimeOffset At(DateOnly date, TimeOnly time)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, _zone.GetUtcOffset(local)).ToUniversalTime();
    }

    private static DateTimeOffset Min(DateTimeOffset left, DateTimeOffset right) => left < right ? left : right;

    private static DateTimeOffset Max(DateTimeOffset left, DateTimeOffset right) => left > right ? left : right;
}
