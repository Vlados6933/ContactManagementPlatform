# Contact Management Platform

A full-stack ASP.NET Core MVC web application for managing contacts (persons) and countries, featuring authentication, role-based authorization, data export, and a comprehensive test suite.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Database Setup](#database-setup)
  - [Running the Application](#running-the-application)
- [Configuration](#configuration)
- [Authentication & Authorization](#authentication--authorization)
- [Data Export](#data-export)
- [Filters & Middleware](#filters--middleware)
- [Testing](#testing)
- [Logging](#logging)

---

## Overview

Contact Management Platform is a CRUD-focused web application built with ASP.NET Core MVC and Entity Framework Core. It allows authenticated users to create, read, update, delete, search, sort, and export contact records. Admins have an isolated area with elevated access. The project demonstrates clean architecture principles with a separation between the Core (domain + business logic), Infrastructure (data access), and UI (presentation) layers.

---

## Architecture

The solution follows a **Clean Architecture / N-Tier** pattern split across three source projects and three test projects:

```
ContactManagementPlatform/
├── src/
│   ├── ContactsManager.Core          # Domain, DTOs, service interfaces & implementations
│   ├── ContactsManager.Infrastructure # EF Core DbContext, repositories, migrations
│   └── ContractsManager.UI           # ASP.NET Core MVC app (controllers, views, filters)
└── tests/
    ├── ContactsManager.ServiceTests      # Unit tests for service layer (xUnit + Moq)
    ├── ContactsManager.ControllerTests   # Unit tests for MVC controllers (xUnit + Moq)
    └── ContactsManager.IntegrationTests  # Integration tests using in-memory DB (xUnit)
```

**Dependency direction:** UI → Core ← Infrastructure. The Core project has no dependency on Infrastructure or UI, keeping domain logic fully isolated.

---

## Project Structure

### `ContactsManager.Core`

| Path | Purpose |
|---|---|
| `Domain/Entities/` | `Person`, `Country` — EF Core domain models |
| `Domain/IdentityEntities/` | `ApplicationUser`, `ApplicationRole` — ASP.NET Identity extensions |
| `Domain/RepositoryContracts/` | `IPersonsRepository`, `ICountriesRepository` — data access abstractions |
| `DTO/` | `PersonAddRequest`, `PersonUpdateRequest`, `PersonResponse`, `CountryAddRequest`, `CountryResponse`, `LoginDTO`, `RegisterDTO` |
| `Enums/` | `GenderOptions`, `SortOrderOptions`, `UserTypeOptions` |
| `Exceptions/` | `InvalidPersonIDException` — custom domain exception |
| `Helpers/` | `ValidationHelper` — model annotation validation utility |
| `ServiceContracs/` | `IPersonsService`, `ICountriesService` — service interfaces |
| `Services/` | `PersonsService`, `CountriesService` — business logic implementations |

### `ContactsManager.Infrastructure`

| Path | Purpose |
|---|---|
| `DbContext/ApplicationDbContext.cs` | EF Core context; extends `IdentityDbContext`; exposes raw SQL stored procedure wrappers |
| `DbContext/DbInitializer.cs` | Runs migrations and seeds `countries.json` / `persons.json` on startup |
| `Migrations/` | EF Core migration history (`Initial`, `IdentityTables`) |
| `Repositories/` | `CountriesRepository`, `PersonsRepository` — EF Core implementations of repository contracts |

### `ContractsManager.UI`

| Path | Purpose |
|---|---|
| `Controllers/PersonsController.cs` | CRUD for contacts; PDF / CSV / Excel export |
| `Controllers/AccountController.cs` | Register, Login, Logout, email uniqueness check |
| `Controllers/CountriesController.cs` | Excel bulk import for countries |
| `Areas/Admin/Controllers/HomeController.cs` | Admin-only area home |
| `Filters/ActionFilters/` | `PersonsListActionFilter`, `PersonCreateAndEditPostActionFilter`, `ResponseHeaderActionFilter` |
| `Filters/AuthorizationFilters/` | `TokenAuthorizationFilter` — cookie-based token check |
| `Filters/ExceptionFilters/` | `HandleExceptionFilter` — dev-mode exception detail renderer |
| `Filters/ResourceFilters/` | `FeatureDisableResourseFilter` — toggleable 501 gate |
| `Filters/ResultFilters/` | `PersonsListResultFilter`, `TokenResultFilter`, `PersonsAlwaysRunResultFilter` |
| `Middleware/ExceptionHandlingMiddleware.cs` | Global exception logger for production |
| `StartupExtensions/ConfigureSreviseExtension.cs` | DI registration and Identity/Authorization configuration |
| `Views/` | Razor views for Persons (Index, Create, Edit, Delete, PersonsPDF), Account (Login, Register), Countries (UploadFromExcel) |
| `wwwroot/` | Static assets: CSS, jQuery, jQuery Validate, Rotativa PDF engine |

---

## Features

**Contact Management**
- List all contacts with server-side search (by name, email, date of birth, gender, country, address) and multi-column sorting (ascending / descending)
- Create a new contact with full validation (required fields, email format, gender selection, country dropdown)
- Edit an existing contact
- Delete a contact with confirmation screen
- Age is calculated automatically from date of birth

**Country Management**
- Bulk import countries from an `.xlsx` file (first sheet named `Countries`, column A)
- Countries seeded automatically from `countries.json` on first startup

**Data Export**
- **PDF** — paginated landscape report via Rotativa / wkhtmltopdf
- **CSV** — download via CsvHelper
- **Excel (.xlsx)** — styled workbook with bold gray header row via EPPlus

**Authentication & Roles**
- Cookie-based authentication with ASP.NET Core Identity
- Two roles: `User` (standard access) and `Admin` (redirected to `/Admin/Home` on login)
- Registration with duplicate email check (remote validation)
- Global fallback policy: all routes require authentication unless explicitly allowed anonymous

**Seed Data**
- Automatically seeds countries and persons from bundled JSON files if the tables are empty

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 (SQL Server provider) |
| Identity | ASP.NET Core Identity with custom `ApplicationUser` / `ApplicationRole` |
| PDF export | Rotativa.AspNetCore 1.4 + wkhtmltopdf |
| CSV export | CsvHelper 33 |
| Excel export / import | EPPlus 8 |
| Logging | Serilog (Console, File, MSSqlServer, Seq sinks) |
| Unit testing | xUnit 2.9, Moq 4.20, AutoFixture 4.18 |
| Integration testing | Microsoft.AspNetCore.Mvc.Testing, EF Core InMemory, Fizzler (CSS selectors on HTML) |

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB (the default connection string targets `(localdb)\MSSQLLocalDB`)
- *(Optional)* [Seq](https://datalust.co/seq) on `http://localhost:5341` for structured log viewing

### Database Setup

No manual SQL script execution is required. On first run, the application:

1. Applies all EF Core migrations automatically (`db.Database.MigrateAsync()`).
2. Seeds the `Countries` table from `ContractsManager.UI/countries.json`.
3. Seeds the `Persons` table from `ContractsManager.UI/persons.json`.

To change the target database, update `ConnectionStrings:DefaultConnection` in `appsettings.json` (see [Configuration](#configuration)).

If you prefer to manage migrations manually:

```bash
# From the solution root
dotnet ef database update --project ContactsManager.Infrastructure --startup-project ContractsManager.UI
```

### Running the Application

```bash
cd ContractsManager.UI
dotnet run
```

The application starts at `https://localhost:5212` (defined in `launchSettings.json`).

On first visit you will be redirected to `/Account/Login`. Register a new account — the first user who registers with `UserType = Admin` is automatically assigned the `Admin` role.

---

## Configuration

All settings live in `ContractsManager.UI/appsettings.json`.

```jsonc
{
  "ConnectionStrings": {
    // SQL Server connection — change Initial Catalog to use a different database name
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ContactsDatabase;..."
  },
  "EPPlus": {
    "ExcelPackage": {
      // EPPlus non-commercial license — replace with a commercial key for production use
      "License": "NonCommercialPersonal:Vlados"
    }
  },
  "Serilog": {
    // Active sinks: Console, Seq (http://localhost:5341)
    // File and MSSqlServer sinks are present but commented out — uncomment to enable
  }
}
```

### Identity Password Policy (configured in `ConfigureSreviseExtension.cs`)

| Setting | Value |
|---|---|
| Minimum length | 5 characters |
| Require digit | No |
| Require uppercase | No |
| Require non-alphanumeric | No |
| Require lowercase | Yes |
| Required unique characters | 3 |

---

## Authentication & Authorization

- **All routes** require an authenticated user by default (global `FallbackPolicy`).
- The `"NotAuthorized"` policy (used on Register/Login pages) is granted only to **unauthenticated** users, preventing authenticated users from re-accessing the auth pages.
- After login, **Admins** are redirected to `/Admin/Home`; regular **Users** go to `/Persons/Index`.
- The `TokenAuthorizationFilter` on `PersonsController.Edit (POST)` additionally checks for an `Auth-Key=A100` cookie. This cookie is set by `TokenResultFilter` on `PersonsController.Edit (GET)`, ensuring the edit form was visited before submission.

---

## Data Export

| Format | Route | Details |
|---|---|---|
| PDF | `GET /Persons/PersonsPDF` | Landscape orientation, 20 px margins, rendered via Rotativa |
| CSV | `GET /Persons/PersonsCSV` | Downloaded as `persons.csv`; columns: Name, Email, Date of Birth, Age, Country, Address, Receive Newsletters |
| Excel | `GET /Persons/PersonsExcel` | Downloaded as `persons.xlsx`; styled header row (bold, light-gray background); auto-fit columns |

---

## Filters & Middleware

### Action Filters

`PersonsListActionFilter` — runs before `Persons/Index`. Populates `ViewBag.SearchFields` and validates/sanitises the `searchBy` parameter to one of the allowed field names.

`PersonCreateAndEditPostActionFilter` — intercepts invalid model state on Create/Edit POST requests, re-populates the countries dropdown, and returns the view immediately without calling the action.

`ResponseHeaderActionFilter` / `ResponseHeaderFilterFactory` — injects custom HTTP response headers at three scopes (global, controller, action) with configurable ordering.

### Authorization Filters

`TokenAuthorizationFilter` — enforces presence and value of the `Auth-Key` cookie; returns 401 if missing or incorrect.

### Resource Filters

`FeatureDisableResourseFilter` — when `isDisabled = true`, short-circuits the pipeline with `501 Not Implemented`. Applied to `Persons/Create (POST)` with `isDisabled = false` (i.e., currently enabled).

### Result Filters

`PersonsListResultFilter` — sets `Last-Modified` response header after `Persons/Index` renders.

`TokenResultFilter` — appends the `Auth-Key=A100` cookie during `Persons/Edit (GET)` response.

`PersonsAlwaysRunResultFilter` — uses `IAsyncAlwaysRunResultFilter` so it executes even when another filter short-circuits; respects `SkipFilter` marker.

### Exception Handling

`HandleExceptionFilter` — returns the exception message as `text/plain` with `500` in Development (commented out on the controller; available for use).

`ExceptionHandlingMiddleware` — logs unhandled exceptions to Serilog in production before re-throwing, preserving the original exception chain.

---

## Testing

The solution contains three test projects, all targeting .NET 10 and using xUnit as the test runner.

### Service Tests (`ContactsManager.ServiceTests`)

Unit tests for `PersonsService` and `CountriesService`. Dependencies (`IPersonsRepository`, `ICountriesRepository`, `ILogger`, `IDiagnosticContext`) are mocked with Moq. Test data is generated with AutoFixture.

```bash
dotnet test ContactsManager.ServiceTests
```

Covered scenarios include:

- `AddPerson` — null argument, null name, and successful creation
- `GetAllPersons` — empty list and populated list
- `GetPersonByPersonID` — null ID, non-existent ID, valid ID
- `GetFilteredPersons` — empty search, filter by name/other fields
- `GetSortedPersons` — ascending and descending by name
- `UpdatePerson` — null argument, invalid ID, successful update
- `DeletePerson` — invalid ID and successful deletion

### Controller Tests (`ContactsManager.ControllerTests`)

Unit tests for `PersonsController` with service layer mocked via Moq.

```bash
dotnet test ContactsManager.ControllerTests
```

### Integration Tests (`ContactsManager.IntegrationTests`)

Uses `WebApplicationFactory` with EF Core InMemory database to spin up the full request pipeline. Fizzler is used to query rendered HTML via CSS selectors.

```bash
dotnet test ContactsManager.IntegrationTests
```

### Run All Tests

```bash
dotnet test
```

---

## Logging

Serilog is configured as the logging provider throughout the application.

| Sink | Status | Notes |
|---|---|---|
| Console | **Active** | All log levels from `Information` up |
| Seq | **Active** | `http://localhost:5341` — requires a running Seq instance |
| File | Commented out | Configurable path, rolling interval, and size limit |
| MSSqlServer | Commented out | Writes to a separate `CRUDLogs` database |

`IDiagnosticContext` (from `Serilog.AspNetCore`) is used in `PersonsService.GetFilteredPersons` and `ExceptionHandlingMiddleware` to attach structured data to the per-request log entry emitted by `UseSerilogRequestLogging()`.

To enable file logging, uncomment the `File` block in `appsettings.json` and adjust the path and `rollingInterval` as needed.
