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
  src/                   Domain, Application, Infrastructure, Api, DbSeeder
  tests/                 Domain, Application and API integration tests
frontend/                React single page application
database/                Generated DDL script, login roles script, demo data
docs/                    Documentation: database, frontend, API specification, personal data
```

## Run locally

Prerequisites: .NET SDK 10, Node.js 24 or later, a PostgreSQL 17 or later database.

1. Create the database structure and the login roles as described in [docs/database/README.md](docs/database/README.md).
2. Configure the API. In development the API reads `backend/src/EquipmentBooking.Api/appsettings.Local.json` (not committed) or environment variables, see [backend/.env.example](backend/.env.example):

   ```json
   {
     "ConnectionStrings": { "Default": "Host=...;Database=...;Username=eb_api;Password=...;SSL Mode=Require" },
     "Jwt": { "SigningKey": "<at least 32 characters>" }
   }
   ```

3. Optionally load the demo data with the seeder ([docs/database/test-data.md](docs/database/test-data.md)).
4. Start the API (http://localhost:5186, reference at http://localhost:5186/docs):

   ```bash
   dotnet run --project backend/src/EquipmentBooking.Api --launch-profile http
   ```

5. Start the frontend (http://localhost:5173; `/api` is proxied to the API):

   ```bash
   cd frontend
   npm ci
   npm run dev
   ```

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
