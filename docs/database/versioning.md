# Versioning of the database structure

The structure is created only by Entity Framework Core migrations stored in Git (`backend/src/EquipmentBooking.Infrastructure/Persistence/Migrations`). Nothing is created by hand.

| Version | Migration | Content |
|---|---|---|
| 1 | `InitialSchema` | Schema `app`, extensions (`btree_gist`, `citext`, `pg_trgm`), 26 tables with keys, constraints, indexes, comments and reference data |
| 2 | `BookingRules` | Exclusion constraint against overlapping bookings, triggers, functions, views, full text and trigram indexes |
| 3 | `DatabaseRoles` | Group roles `app_read`, `app_write`, `app_admin` and their privileges |

## Rules

- One migration per change, named after the change; applied migrations are never edited, corrections are new migrations.
- Parts that EF Core cannot model (exclusion constraint, triggers, functions, views, roles) are versioned SQL files in `Persistence/Sql`, embedded in the assembly and executed by the migration; every migration has a down script.
- The history of applied migrations is stored in `app."__EFMigrationsHistory"`.
- `database/ddl/equipment_booking.sql` is the complete idempotent DDL script generated from the migrations and is regenerated with every migration:

```bash
dotnet ef migrations script --idempotent --project backend/src/EquipmentBooking.Infrastructure --startup-project backend/src/EquipmentBooking.Infrastructure -o database/ddl/equipment_booking.sql
```

- The data dictionary tables are regenerated from the catalog with `database/scripts/generate_data_dictionary.sql`.

## Apply the migrations

```bash
export ConnectionStrings__Migrations="Host=...;Database=...;Username=<owner>;Password=...;SSL Mode=Require"
dotnet ef database update --project backend/src/EquipmentBooking.Infrastructure --startup-project backend/src/EquipmentBooking.Infrastructure
```

Alternatively, run `database/ddl/equipment_booking.sql` with psql as the owner.
