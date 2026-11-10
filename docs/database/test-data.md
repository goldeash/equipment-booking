# Test and demo data

## Reference data

Fixed reference values (roles, statuses, conditions, maintenance types, notification types, decision sources, routing reasons) and the default organisation settings are inserted by the initial migration and are part of `database/ddl/equipment_booking.sql`.

## Demo data

`backend/src/EquipmentBooking.DbSeeder` generates a deterministic, realistic data set relative to the day it runs. All people are fictional and use the reserved domain `company.example`.

| Data | Volume |
|---|---|
| Departments | 8 |
| Users | 60 (2 administrators, 8 department managers, 50 employees) |
| Categories | 14, including 3 specialised categories |
| Locations and features | 6 and 20 |
| Items | 140 |
| Bookings | about 3,600 over the last six months and the next 40 days |
| Maintenance records | about 300 |

Edge cases covered: returned loans on time and late, damaged returns followed by repairs, current and overdue loans, declined and cancelled requests, approved and still requested future bookings, items out of service now, high value and specialised items with every routing reason, notifications read and unread.

Load the data (the owner connection and the password of the three demo accounts come from environment variables):

```bash
export ConnectionStrings__Migrations="Host=...;Database=...;Username=<owner>;Password=...;SSL Mode=Require"
export EB_DEMO_PASSWORD=...
dotnet run --project backend/src/EquipmentBooking.DbSeeder -- --export database/seed/demo_data.sql
```

`database/seed/demo_data.sql` is the exported script of the same data. It contains no password hashes; the seeder sets the passwords of the demo accounts `admin@company.example`, `manager@company.example` and `employee@company.example` after loading.

## Automated tests

The integration tests (`backend/tests/EquipmentBooking.Api.IntegrationTests`) rebuild a separate test database from the migrations and insert their own small data set before each run.
