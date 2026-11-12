# Versioning of the database structure

The structure is created only by Entity Framework Core migrations stored in Git (`backend/src/EquipmentBooking.Infrastructure/Persistence/Migrations`). Nothing is created by hand.

| Version | Migration | Content |
|---|---|---|
| 1 | `InitialSchema` | Schema `app`, extensions (`btree_gist`, `citext`, `pg_trgm`), 26 tables with keys, constraints, indexes, comments and reference data |
| 2 | `BookingRules` | Exclusion constraint against overlapping bookings, triggers, functions, views, full text and trigram indexes |
| 3 | `DatabaseRoles` | Group roles `app_read`, `app_write`, `app_admin` and their privileges |
| 4 | `BookingExpiry` | Notification types for requests that expired without a decision and bookings that were not collected |
| 5 | `IntegrityRules` | Reference tables for every enumeration, allowed status transitions, cancellation reasons, one row per policy check with the compared values, settings history; triggers for status transitions, booking consistency, overdue blocking, maintenance overlaps from both sides and the roles of actors; `RESTRICT` on audit data; least privilege grants. Existing data are converted and validated in the same migration |

## Rules

- One migration per change, named after the change; applied migrations are never edited, corrections are new migrations.
- Parts that EF Core cannot model (exclusion constraint, triggers, functions, views, roles) are versioned SQL files in `Persistence/Sql`, embedded in the assembly and executed by the migration; every migration has a down script.
- The history of applied migrations is stored in `app."__EFMigrationsHistory"`.
- `database/ddl/equipment_booking.sql` is the complete idempotent DDL script generated from the migrations and is regenerated with every migration:

```bash
cd backend
dotnet tool restore
dotnet ef migrations script --idempotent --project src/EquipmentBooking.Infrastructure --startup-project src/EquipmentBooking.Infrastructure -o ../database/ddl/equipment_booking.sql
```

- The generated sections of the data dictionary and the privilege matrix are regenerated from the catalog with `database/scripts/generate_data_dictionary.sql` and `database/scripts/generate_privileges.sql`.
- A down migration restores the previous structure and converts the data back as far as the previous structure can hold them.

## Apply the migrations

```bash
cd backend
dotnet tool restore
export ConnectionStrings__Migrations="Host=<host>;Database=<database>;Username=<owner>;Password=<owner password>"
dotnet ef database update --project src/EquipmentBooking.Infrastructure --startup-project src/EquipmentBooking.Infrastructure
```

Alternatively, run `database/ddl/equipment_booking.sql` with psql as the owner.
