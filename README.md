# The Unheard Archive API

Current .NET backend for The Unheard Archive. The frontend is maintained separately in `../the-unheard-archive`; the sibling `../Tywynh.API` checkout is an older implementation.

## Architecture and requirements

.NET 8 ASP.NET Core, MediatR application handlers, domain entities, EF Core/Npgsql repositories, and PostgreSQL. Use a compatible .NET SDK and PostgreSQL 16 for local checks. The API includes owner workflow controllers/services for receipt management, consent-aware publication records, and concerns. AI reflections are disabled by default.

## Configuration

Runtime values can be supplied through standard .NET environment variables. See `Tywynh.API/appsettings.example.json` for the configuration structure and the frontend `docs/OPERATIONS.md` for the complete runbook.

- `ConnectionStrings__DefaultConnection`: explicitly chosen PostgreSQL database.
- `Gateway__SharedKey`: at least 32 characters; matches the frontend's server-only gateway key.
- `Receipts__SigningKey`: durable, independent receipt signing key; at least 32 bytes.
- `Moderation__AdminKey`: separate protected moderation key; injected by the frontend server.
- `Features__ReflectionsEnabled`: false until deliberately enabled with a configured Gemini key.
- `Hosting__KnownProxies__0`: explicitly verified reverse proxies, when applicable.

Production API calls fail closed unless authenticated by the archive gateway. Health endpoints remain accessible for readiness/liveness checks. Do not expose keys, receipt tokens, cookies, or story text in logs.

## Build and run locally

```powershell
dotnet restore Tywynh.API/Tywynh.API.csproj
dotnet build Tywynh.API/Tywynh.API.csproj -c Release --no-restore
dotnet run --project Tywynh.API --no-launch-profile
```

Set environment values before starting; use a disposable localhost database for development checks. Configure `ASPNETCORE_URLS` to a loopback address when previewing locally.

## Migrations and verification

There are no automatic startup migrations. EF tooling requires an explicit `ConnectionStrings__DefaultConnection`; it does not discover a sibling checkout's configuration. Apply migrations only to a deliberately selected database after the required backup and authorization.

The critical-flow runner refuses remote hosts and database names without `_test`. It creates a unique schema, applies migrations there, starts a localhost API, exercises public boundaries/contracts/idempotency/consent/receipts/concurrency/reporting/rate limits, and cleans up its schema.

```powershell
# Set ARCHIVE_TEST_DB to the connection string of your disposable localhost *_test database.
dotnet run --project Tests/ArchiveChecks -c Release
```

For restricted environments, read-only Git inspection works with `git --git-dir=.git --work-tree=. status --short`. No Git metadata repair was necessary during implementation.

## Release constraints

Verify real API/database hosting, TLS termination, backup retention, staging, and rollback before release. The contributor-workflow migration is additive and does not infer permission for legacy writing. Do not run the prototype under `Tools/DropTables`. Do not deploy, migrate production, or post externally without separate authorization.

See the frontend `docs/IMPLEMENTATION_REPORT.md`, `docs/IMPROVEMENT_PLAN.md`, and `docs/PRODUCT_DIRECTION.md` for project status and editorial operation.