# Test and demo data

## Reference data

Fixed reference values (roles, statuses, conditions, maintenance types, notification types, decision sources, routing reasons) and the default organisation settings are inserted by the migrations and are part of `database/ddl/equipment_booking.sql`. A database created from the migrations is ready for use without any further reference data scripts.

## Demo data

The console tool `backend/tools/EquipmentBooking.DbSeeder` generates a realistic data set and loads it into the database. It is not part of the API and is never deployed with it.

The data come only from the repository: the departments, locations, categories, item models with their values and the booking purposes are listed in `DemoCatalogue.cs`; the names of the people are produced by the Bogus library with a fixed random seed; the rules in `DemoDataGenerator.cs` decide how often items are booked, for how long and with which outcome. Every run therefore produces the same data set, with all dates relative to the day of the run, so the database always contains past loans, current loans, recent overdue loans and requests waiting for a decision. All people are fictional and use the reserved domain `company.example`.

| Data | Volume |
|---|---|
| Departments | 8 |
| Users | 60 (2 administrators, 8 department managers, 50 employees) |
| Categories | 14, including 3 specialised categories |
| Locations and features | 6 and 20 |
| Items | 140 |
| Bookings | about 3,600 over the last six months and the next 40 days |
| Maintenance records | about 270 |

Edge cases covered: returned loans on time and late, damaged returns followed by repairs, current and overdue loans, declined and cancelled requests, approved and still requested future bookings, items out of service now, high value and specialised items with every routing reason, notifications read and unread.

## Load the demo data

The seeder needs the .NET SDK 10 and a database created from the migrations. It connects as the schema owner, replaces all application data in one transaction (reference data stay unchanged) and sets the password of the three demo accounts:

```bash
export ConnectionStrings__Migrations="Host=localhost;Database=equipment_booking;Username=<owner>;Password=<owner password>"
export EB_DEMO_PASSWORD="<password of the demo accounts, at least 12 characters>"
dotnet run --project backend/tools/EquipmentBooking.DbSeeder
```

| Account | Role |
|---|---|
| `admin@company.example` | Administrator |
| `manager@company.example` | Department manager (Engineering) |
| `employee@company.example` | Employee (Engineering) |

The generated data can also be written to a SQL file for inspection without loading it: `dotnet run --project backend/tools/EquipmentBooking.DbSeeder -- --export demo_data.sql --no-load`. The file contains no password hashes, so it is not suitable for signing in.

## Automated tests

The integration tests (`backend/tests/EquipmentBooking.Api.IntegrationTests`) rebuild a separate test database from the migrations and insert their own small data set before each run.
