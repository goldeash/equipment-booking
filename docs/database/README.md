# Database

PostgreSQL 18 hosted on Neon (any PostgreSQL 17 or later works), accessed with Entity Framework Core 10 through the Npgsql provider.

| Document | Content |
|---|---|
| [data-dictionary.md](data-dictionary.md) | Tables, columns, types, keys, constraints, data quality expectations, reference values, relationships |
| [logical-schema.md](logical-schema.md) | Colour coded block diagram and entity relationship diagram |
| [integrity-and-transactions.md](integrity-and-transactions.md) | Constraints, triggers, functions, views, indexes and transaction handling |
| [roles-and-access.md](roles-and-access.md) | Database roles and privileges |
| [versioning.md](versioning.md) | Migrations, DDL script and how to apply them |
| [test-data.md](test-data.md) | Reference data, demo data and test data |

| Artefact | Location |
|---|---|
| Migrations | `backend/src/EquipmentBooking.Infrastructure/Persistence/Migrations` |
| SQL parts of migrations | `backend/src/EquipmentBooking.Infrastructure/Persistence/Sql` |
| Entity configurations with column comments | `backend/src/EquipmentBooking.Infrastructure/Persistence/Configurations` |
| Complete DDL script | `database/ddl/equipment_booking.sql` |
| Login roles script | `database/scripts/create_login_roles.sql` |
| Data dictionary generator | `database/scripts/generate_data_dictionary.sql` |
| Demo data | `backend/src/EquipmentBooking.DbSeeder`, `database/seed/demo_data.sql` |

## Create a database from scratch

1. Create an empty database and connect as its owner.
2. Apply the migrations (see [versioning.md](versioning.md)) or run `database/ddl/equipment_booking.sql`.
3. Create the login roles with `database/scripts/create_login_roles.sql`.
4. Load the demo data with the seeder (see [test-data.md](test-data.md)).
5. Point the API to the database with the `eb_api` login (`ConnectionStrings__Default`).
