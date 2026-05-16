# Unheard Archives Backend

Backend API for the Tywynh Unheard Archives application, built with .NET 8 and PostgreSQL.

## Overview

This solution implements a clean architecture-style backend with separate layers for API, application logic, domain entities, and infrastructure.

### Key components

- `Tywynh.API/` - ASP.NET Core Web API host and controller layer
- `Tywynh.Application/` - application services, commands, queries, and DTOs
- `Tywynh.Domain/` - core domain entities, enums, interfaces, and value objects
- `Tywynh.Infrastructure/` - data access, EF Core persistence, and database context

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL database
- Git

### Setup

1. Restore packages:

```bash
dotnet restore
```

2. Copy the example configuration:

```bash
copy Tywynh.API\appsettings.example.json Tywynh.API\appsettings.json
```

3. Update `Tywynh.API/appsettings.json` with your PostgreSQL connection details.

4. Run the API:

```bash
dotnet run --project Tywynh.API\Tywynh.API.csproj
```

## Configuration

- `Tywynh.API/appsettings.json` is treated as a local configuration file and is ignored by git.
- Use `Tywynh.API/appsettings.example.json` as the template for local setup.
- Do not commit production connection strings or secret credentials.

## Project Structure

- `Tywynh.API/` - API startup, controllers, middleware
- `Tywynh.Application/` - business rules, command/query handlers, dependency injection registration
- `Tywynh.Domain/` - entities, enums, repository contracts, domain exceptions
- `Tywynh.Infrastructure/` - EF Core DbContext, repositories, persistence implementation

## Notes

- Keep secret values out of source control.
- Use environment variables or user secrets for production deployments.
- `appsettings.Development.json` can be used for development-specific settings without exposing secrets.
