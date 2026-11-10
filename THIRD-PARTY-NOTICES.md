# Third-party software

The project uses only free and open source packages. Exact versions are pinned in `backend/Directory.Packages.props`, `backend/.config/dotnet-tools.json` and `frontend/package-lock.json`.

## Backend (NuGet)

| Package | Version | Licence |
|---|---|---|
| Microsoft.AspNetCore.OpenApi | 10.0.15 | MIT |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.15 | MIT |
| Microsoft.Extensions.ApiDescription.Server | 10.0.15 | MIT |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 10.0.15 | MIT |
| Microsoft.Extensions.Options | 10.0.15 | MIT |
| Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Design | 10.0.15 | MIT |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | PostgreSQL License |
| EFCore.NamingConventions | 10.0.1 | Apache-2.0 |
| FluentValidation, FluentValidation.DependencyInjectionExtensions | 12.2.0 | Apache-2.0 |
| Serilog.AspNetCore | 10.0.0 | Apache-2.0 |
| Serilog.Formatting.Compact | 3.0.0 | Apache-2.0 |
| Scalar.AspNetCore | 2.20.0 | MIT |
| Bogus | 35.6.5 | MIT |
| xunit.v3 | 4.0.3 | Apache-2.0 |
| NSubstitute | 6.2.0 | BSD-3-Clause |
| Shouldly | 4.3.0 | BSD-3-Clause |
| coverlet.MTP | 10.1.0 | MIT |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.15 | MIT |
| Microsoft.Extensions.TimeProvider.Testing | 10.11.0 | MIT |
| dotnet-ef (tool) | 10.0.15 | MIT |
| dotnet-reportgenerator-globaltool (tool) | 5.5.12 | Apache-2.0 |

## Frontend (npm)

| Package | Version | Licence |
|---|---|---|
| @hookform/resolvers | 7.0.5 | MIT |
| @mantine/core | 9.8.0 | MIT |
| @mantine/dates | 9.8.0 | MIT |
| @mantine/hooks | 9.8.0 | MIT |
| @mantine/notifications | 9.8.0 | MIT |
| @tabler/icons-react | 3.51.0 | MIT |
| @tanstack/react-query | 5.107.1 | MIT |
| dayjs | 1.11.23 | MIT |
| react | 19.3.1 | MIT |
| react-dom | 19.3.1 | MIT |
| react-hook-form | 7.91.0 | MIT |
| react-router | 8.5.1 | MIT |
| zod | 4.6.7 | MIT |
| zustand | 5.1.0 | MIT |
| @eslint/js | 10.0.1 | MIT |
| @testing-library/jest-dom | 7.0.1 | MIT |
| @testing-library/react | 16.3.3 | MIT |
| @testing-library/user-event | 14.6.9 | MIT |
| @types/node | 24.19.2 | MIT |
| @types/react | 19.3.0 | MIT |
| @types/react-dom | 19.3.0 | MIT |
| @vitejs/plugin-react | 6.3.0 | MIT |
| @vitest/coverage-v8 | 5.1.0 | MIT |
| eslint | 10.14.0 | MIT |
| eslint-config-prettier | 10.1.8 | MIT |
| eslint-plugin-react-hooks | 7.1.1 | MIT |
| eslint-plugin-react-refresh | 0.5.7 | MIT |
| globals | 17.14.0 | MIT |
| jsdom | 31.0.0 | MIT |
| msw | 3.0.4 | MIT |
| postcss | 8.5.32 | MIT |
| postcss-preset-mantine | 1.18.0 | MIT |
| postcss-simple-vars | 7.0.1 | MIT |
| prettier | 3.9.10 | MIT |
| typescript | 6.0.3 | Apache-2.0 |
| typescript-eslint | 8.72.2 | MIT |
| vite | 8.3.7 | MIT |
| vitest | 5.1.0 | MIT |

## Database

PostgreSQL with the extensions btree_gist, citext and pg_trgm (PostgreSQL License).
