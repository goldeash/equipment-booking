# Equipment Booking

Web-based equipment reservation and inventory management system with AI-assisted request handling. Employees find shared equipment and book it for date ranges, overlapping bookings are rejected, routine requests can be approved automatically and every other request goes to the department manager, administrators issue and receive items and keep the condition and maintenance history.

Bachelor diploma project, European Humanities University, by Denis Ivanets.

## Features

- Catalogue search by keyword, category, location, features and availability on dates
- Booking requests that block the item; overlaps are rejected by the database, also for simultaneous requests
- Approval route with fixed policy checks (value threshold, specialised items, loan duration, approval switches) and a decision log; the AI purpose check is planned, until then routine requests are routed to the manager
- Manager queue with the reasons for routing and the requester's return history; decline only with a comment
- Issue and return with condition, automatic overdue detection, return reminders and in-app notifications
- Automatic cancellation of requests not decided by the start date and of bookings not collected by the end date
- Maintenance records that take items out of service and cancel affected bookings after confirmation
- Users, roles (employee, department manager, administrator), departments and organisation settings
- Privacy notice, role based access and anonymisation of personal data after the retention period

## Technology

| Part | Technology |
|---|---|
| Backend | ASP.NET Core on .NET 10, layered architecture (Domain, Application, Infrastructure, Api), Entity Framework Core 10, FluentValidation, Serilog, JWT bearer authentication |
| Database | PostgreSQL 18 (Neon), migrations, exclusion constraint, triggers, views, database roles |
| Frontend | React 19, TypeScript, Vite, Mantine, TanStack Query, Zustand, react-hook-form, zod |
| API documentation | OpenAPI 3.1 generated at build (`docs/api/openapi.json`), interactive reference at `/docs` |
| Tests | xUnit v3, NSubstitute, Shouldly, coverlet; Vitest, Testing Library, MSW |

## Repository structure

```
backend/                 ASP.NET Core solution (EquipmentBooking.slnx)
  src/                   Domain, Application, Infrastructure, Api
  tests/                 Domain, Application and API integration tests
  tools/                 Demo data generator (not deployed)
frontend/                React single page application
database/                DDL script generated from the migrations, login roles script, data dictionary query
docs/                    Documentation: database, frontend, API specification, personal data
```

## Run locally

Prerequisites: .NET SDK 10, Node.js 24 or later, PostgreSQL 17 or later with `psql`.

### 1. Create and fill the database

Create an empty database; the user that creates it becomes its owner and runs the next steps (no password is stored in the repository, every value below is chosen by you):

```bash
createdb equipment_booking
```

Create the structure and the reference data from the migrations:

```bash
cd backend
dotnet tool restore
export ConnectionStrings__Migrations="Host=localhost;Database=equipment_booking;Username=<owner>;Password=<owner password>"
dotnet ef database update --project src/EquipmentBooking.Infrastructure --startup-project src/EquipmentBooking.Infrastructure
cd ..
```

Without the .NET tools the same structure is created by `psql -d equipment_booking -f database/ddl/equipment_booking.sql`.

Create the login roles of the application:

```bash
psql -d equipment_booking -v api_password=<eb_api password> -v readonly_password=<eb_readonly password> -f database/scripts/create_login_roles.sql
```

Load the demo data (60 users, 140 items, about 3,600 bookings; details in [docs/database/test-data.md](docs/database/test-data.md)), still with `ConnectionStrings__Migrations` set:

```bash
export EB_DEMO_PASSWORD="<password of the demo accounts, at least 12 characters>"
dotnet run --project backend/tools/EquipmentBooking.DbSeeder
```

The demo accounts are `admin@company.example`, `manager@company.example` and `employee@company.example` with the password from `EB_DEMO_PASSWORD`. All database documents are listed in [docs/database/README.md](docs/database/README.md).

### 2. Configure and start the API

In development the API reads `backend/src/EquipmentBooking.Api/appsettings.Local.json` (not committed) or environment variables, see [backend/.env.example](backend/.env.example). It connects with the `eb_api` login, never as the owner; add `SSL Mode=Require` for a hosted database:

```json
{
  "ConnectionStrings": { "Default": "Host=localhost;Database=equipment_booking;Username=eb_api;Password=<eb_api password>" },
  "Jwt": { "SigningKey": "<at least 32 characters>" }
}
```

```bash
dotnet run --project backend/src/EquipmentBooking.Api --launch-profile http
```

The API listens on http://localhost:5186 with the interactive reference at http://localhost:5186/docs.

### 3. Start the frontend

```bash
cd frontend
npm ci
npm run dev
```

Open http://localhost:5173 and sign in with a demo account; `/api` is proxied to the API.

## Tests

```bash
dotnet test --solution backend/EquipmentBooking.slnx
```

The integration tests need a separate PostgreSQL test database given by `EB_TEST_DB_OWNER_CONNECTION` and `EB_TEST_DB_APP_CONNECTION`; without them they are skipped. Coverage of the business logic (Domain and Application):

```bash
dotnet test --solution backend/EquipmentBooking.slnx --coverlet --coverlet-output-format cobertura --coverlet-include "[EquipmentBooking.Domain]*" --coverlet-include "[EquipmentBooking.Application]*"
```

Frontend:

```bash
cd frontend
npm test
npm run lint
```

## Documentation

| Topic | Location |
|---|---|
| Database: data dictionary, schema, integrity, roles, versioning, test data | [docs/database](docs/database/README.md) |
| Frontend architecture, flows and tests | [docs/frontend](docs/frontend/README.md) |
| API specification | [docs/api/openapi.json](docs/api/openapi.json) |
| Personal data and processors | [docs/legal/processors.md](docs/legal/processors.md) |
| Third-party licences | [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) |
