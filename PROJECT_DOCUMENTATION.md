# Smart Notification System — Complete Project Documentation

> **Purpose:** Full record of the project — what it does, what was built, every enhancement made, and how to run it.
> **Target:** Germany .NET job market portfolio project.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Original Project State](#2-original-project-state)
3. [Phase 1 — Production-Ready Enhancements](#3-phase-1--production-ready-enhancements)
4. [Phase 2 — Advanced Enterprise Features](#4-phase-2--advanced-enterprise-features)
5. [Final Project Structure](#5-final-project-structure)
6. [API Reference](#6-api-reference)
7. [How to Run](#7-how-to-run)
8. [How to Test](#8-how-to-test)
9. [Technology Stack](#9-technology-stack)
10. [Key Concepts for Interviews](#10-key-concepts-for-interviews)

---

## 1. Project Overview

**Smart Notification System** is a production-grade ASP.NET Core 8 Web API for creating, managing, and delivering notifications (Email, SMS, Push, Webhook). It demonstrates enterprise .NET architecture patterns valued in the German tech market.

### What It Does
- Users authenticate with JWT (access + refresh tokens)
- Admins create notifications with type, priority, and optional scheduling
- A background worker polls for pending notifications, delivers them, and retries on failure
- Connected browser/app clients receive real-time updates via SignalR when a notification is processed
- All operations are logged with structured JSON logs (Serilog) including correlation IDs

---

## 2. Original Project State

### What Existed (Before Enhancements)
- Basic ASP.NET Core 8 Web API
- JWT authentication (hardcoded secret key — security risk)
- Plain-text password storage (no hashing)
- Login via query string (security risk)
- **In-memory database** — no persistence
- Background worker with no retry logic
- `Console.WriteLine` for logging
- No input validation on DTOs
- No Swagger JWT support
- No unit or integration tests
- No Docker
- No CORS, rate limiting, or health checks
- Namespace inconsistency (`Model` vs `Models`)
- Controller directly querying the database, bypassing service/repository layers

### Original File List
```
Controllers/AuthController.cs
Controllers/NotificationController.cs
Models/User.cs              (stored plain-text password)
Models/Notification.cs
Models/AppDBContext.cs
Services/JwtService.cs
Services/NotificationServices.cs
Repositories/NotificationRepository.cs
Workers/NotificationWorker.cs
Middelwares/ExceptionMiddleware.cs
Filters.cs                  (Console.WriteLine logging)
DTO/NotificationRequestDto.cs
Program.cs
appsettings.json
appsettings.Development.json
```

---

## 3. Phase 1 — Production-Ready Enhancements

### 3.1 Security Fixes

| Problem | Fix |
|---------|-----|
| Hardcoded JWT secret in `Program.cs` | Moved to `appsettings.json` (`Jwt:Key`) |
| JWT did NOT validate issuer, audience, or lifetime | All three now validated; `ClockSkew = Zero` |
| Plain-text passwords | BCrypt hashing on register; `BCrypt.Verify` on login |
| Login via query string (`?username=&password=`) | Changed to `[FromBody] LoginRequestDto` |
| No input validation | `[Required]`, `[MaxLength]`, `[RegularExpression]` on all DTOs |

### 3.2 Database Switch

- Replaced **in-memory database** with **SQL Server (LocalDB)**
- Added EF Core `OnModelCreating` with proper indexes on `IsProcessed` and `CreatedAt`
- Added **seed data** at startup: creates `admin` / `Admin@123` if no users exist
- Added `Microsoft.EntityFrameworkCore.Tools` for migrations support

### 3.3 Model Changes

**User.cs**
```
Added: PasswordHash (replaces Password)
Added: CreatedAt
```

**Notification.cs**
```
Added: IsFailed       — permanently failed after max retries
Added: ProcessedAt    — timestamp when processed
Added: ScheduledAt    — optional future delivery time
Added: IsDeleted      — soft delete flag
Fixed: Namespace standardized to Smart_Notification_System.Models
```

### 3.4 New DTOs

| File | Purpose |
|------|---------|
| `LoginRequestDto.cs` | Body-based login |
| `RegisterRequestDto.cs` | User registration |
| `NotificationResponseDto.cs` | Response — never exposes raw entities |
| `PagedResponseDto<T>.cs` | Pagination wrapper with TotalPages, HasNextPage |
| `UpdateNotificationDto.cs` | Partial update (all fields optional) |

### 3.5 Repository Enhancements

`NotificationRepository` gained:
- `GetByIdAsync(int id)` — single notification by ID
- `GetPagedAsync(page, pageSize, type, priority, isProcessed)` — filtered + paginated
- `GetPendingAsync(maxRetryCount)` — respects `ScheduledAt` and `IsFailed`
- `UpdateAsync(Notification n)` — update existing
- `SoftDeleteAsync(int id)` — sets `IsDeleted = true`

### 3.6 API Endpoints Added

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/auth/register` | Public | Register new user |
| `POST` | `/api/auth/login` | Public | Login (body, not query string) |
| `GET` | `/api/notifications` | Any user | Paginated + filtered list |
| `GET` | `/api/notifications/{id}` | Any user | Single notification |
| `PATCH` | `/api/notifications/{id}` | Admin | Partial update |
| `DELETE` | `/api/notifications/{id}` | Admin | Soft delete |

### 3.7 Infrastructure Additions

- **Rate Limiting** — Fixed window: 100 req / 60s (configurable in `appsettings.json`)
- **CORS** — Named `DefaultCors` policy
- **Health Check** — `GET /health` checks database connectivity
- **Swagger + JWT** — Bearer token input in Swagger UI; Swagger served at root `/`
- **ProblemDetails** — `ExceptionMiddleware` returns RFC 7807 `application/problem+json`
- **ILogger everywhere** — `Console.WriteLine` replaced with `ILogger<T>` in all classes

### 3.8 Background Worker Improvements

- Worker now reads `ScheduledAt` — notifications are skipped until their scheduled time
- Increments `RetryCount` on failure
- Sets `IsFailed = true` after reaching `MaxRetryCount`
- `MaxRetryCount` and `IntervalSeconds` configurable via `appsettings.json`

---

## 4. Phase 2 — Advanced Enterprise Features

### Prerequisite — `INotificationRepository` Interface

**Why:** Required for unit testing (NSubstitute mocking) and proper Dependency Inversion.

**Changes:**
- Created `Repositories/INotificationRepository.cs` with all method signatures
- `NotificationRepository` now implements `INotificationRepository`
- DI registration changed from `AddScoped<NotificationRepository>()` to `AddScoped<INotificationRepository, NotificationRepository>()`
- `NotificationService` and `NotificationWorker` now depend on the interface, not the concrete class

---

### Feature 1 — CQRS with MediatR

**Why it matters for Germany:** CQRS is considered baseline architecture knowledge in German enterprise .NET roles (banking, insurance, logistics). Nearly every German senior .NET interview asks about it.

**Package added:** `MediatR 12.x`

**New folder structure:**
```
Application/
  Notifications/
    Commands/
      CreateNotificationCommand.cs
      CreateNotificationCommandHandler.cs
      UpdateNotificationCommand.cs
      UpdateNotificationCommandHandler.cs
      DeleteNotificationCommand.cs
      DeleteNotificationCommandHandler.cs
    Queries/
      GetNotificationByIdQuery.cs
      GetNotificationByIdQueryHandler.cs
      GetPagedNotificationsQuery.cs
      GetPagedNotificationsQueryHandler.cs
    NotificationMapper.cs
  Common/
    Behaviors/
      ValidationBehavior.cs
```

**Key concepts:**
- Each use case = one `IRequest<T>` record + one `IRequestHandler<TRequest, TResponse>` class
- Controllers no longer call services — they call `_mediator.Send(new XxxCommand(...))`
- `NotificationService` was dissolved into individual handlers
- `ValidationBehavior<TRequest, TResponse>` is an `IPipelineBehavior` that runs FluentValidation before every handler

**Controller before CQRS:**
```csharp
var result = await _service.CreateAsync(dto);
```

**Controller after CQRS:**
```csharp
var command = new CreateNotificationCommand(dto.Message, dto.Type, dto.Priority, dto.ScheduledAt);
var result = await _mediator.Send(command);
```

---

### Feature 2 — Serilog Structured Logging

**Why it matters for Germany:** German enterprises (SAP, automotive, finance) run centralized log aggregation (ELK, Splunk, Seq). Serilog is the de-facto standard.

**Packages added:**
- `Serilog.AspNetCore`
- `Serilog.Sinks.File`
- `Serilog.Enrichers.Environment`
- `Serilog.Enrichers.Thread`

**New file:** `Middlewares/CorrelationIdMiddleware.cs`
- Reads or generates `X-Correlation-Id` header
- Pushes it to `LogContext` so every log line includes the correlation ID
- Echoes correlation ID back in the response header

**Configuration in `appsettings.json`:**
```json
"Serilog": {
  "MinimumLevel": { "Default": "Information", "Override": { "Microsoft": "Warning" } },
  "WriteTo": [
    { "Name": "Console", "Args": { "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}" } },
    { "Name": "File", "Args": { "path": "logs/notification-.log", "rollingInterval": "Day", "retainedFileCountLimit": 7 } }
  ],
  "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
}
```

**Log output includes:** Timestamp, Level, CorrelationId, MachineName, ThreadId, Message

**Zero code changes needed** — Serilog plugs into Microsoft's `ILogger` abstraction.

---

### Feature 3 — FluentValidation with MediatR Pipeline

**Why it matters for Germany:** Data annotation validation is considered junior-level. FluentValidation with a MediatR pipeline behavior is the expected enterprise pattern.

**Packages added:**
- `FluentValidation.AspNetCore`
- `FluentValidation.DependencyInjectionExtensions`

**New validators:**
```
Application/Notifications/Validators/
  CreateNotificationCommandValidator.cs
  UpdateNotificationCommandValidator.cs
```

**Example validator rules:**
```csharp
RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
RuleFor(x => x.Type).Must(t => new[]{"Email","SMS","Push","Webhook"}.Contains(t));
RuleFor(x => x.Priority).Must(p => new[]{"Low","Normal","High","Critical"}.Contains(p));
RuleFor(x => x.ScheduledAt).GreaterThan(DateTime.UtcNow).When(x => x.ScheduledAt.HasValue);
```

**`ValidationBehavior` pipeline:**
1. Receives `TRequest` before the handler runs
2. Runs all `IValidator<TRequest>` instances registered in DI
3. If any fail → throws `ValidationException`
4. `ExceptionMiddleware` catches it → returns `422 Unprocessable Entity` with field-level errors:

```json
{
  "status": 422,
  "title": "Validation Failed",
  "errors": {
    "Priority": ["Priority must be one of: Low, Normal, High, Critical."]
  }
}
```

**Data annotations removed** from all DTOs — validation lives in one place.

---

### Feature 4 — JWT Refresh Tokens

**Why it matters for Germany:** Security-conscious German employers (fintech, healthcare, insurance) always ask about token lifecycle management.

**Model changes (User.cs):**
```csharp
public string? RefreshTokenHash { get; set; }   // stored as SHA-256 hash
public DateTime? RefreshTokenExpiry { get; set; }
```

**New DTO:** `RefreshTokenRequestDto.cs`

**JwtService changes:**
- `GenerateAccessToken(User user)` — 15-minute expiry (tightened from 1 hour)
- `GenerateRefreshToken()` — cryptographically random 64-byte Base64 string
- `HashRefreshToken(string token)` — SHA-256 hash for secure storage

**New endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/auth/login` | Returns `accessToken` + `refreshToken` |
| `POST` | `/api/auth/refresh` | Exchange refresh token for new access token (rotates refresh token) |
| `POST` | `/api/auth/logout` | Clears refresh token (revocation without a blacklist) |

**Security model:**
- Access token: short-lived (15 min), stored in memory by client
- Refresh token: long-lived (7 days), stored hashed in DB
- On refresh: old token invalidated, new token issued (rotation)
- On logout: refresh token nulled in DB

---

### Feature 5 — Unit & Integration Tests

**Why it matters for Germany:** German companies have strict quality gates. Test coverage is mandatory. A separate test project with passing tests is a direct hiring differentiator.

**New project:** `Smart_Notification_System.Tests`

**Packages:**
- `xunit` — test framework
- `NSubstitute` — mocking library
- `FluentAssertions` — readable assertions
- `Microsoft.AspNetCore.Mvc.Testing` — integration test WebApplicationFactory
- `Microsoft.EntityFrameworkCore.InMemory` — in-memory DB for integration tests
- `coverlet.collector` — code coverage

**Test files:**

| File | What It Tests |
|------|--------------|
| `Unit/Commands/CreateNotificationCommandHandlerTests.cs` | Handler creates notification, sets fields correctly |
| `Unit/Commands/DeleteNotificationCommandHandlerTests.cs` | Handler deletes existing; returns false for non-existent |
| `Unit/Workers/NotificationWorkerTests.cs` | Worker marks notifications processed; no-op when empty |
| `Unit/Services/JwtServiceTests.cs` | Token generation, refresh token randomness, hash consistency, short-key exception |
| `Integration/AuthApiIntegrationTests.cs` | Login success/fail, register, duplicate username, refresh token |
| `Integration/NotificationApiIntegrationTests.cs` | CRUD, auth enforcement (401/403), FluentValidation (422), pagination |

**Total: 22 tests — all passing**

**`CustomWebApplicationFactory`** replaces SQL Server with InMemory DB for tests. The app's own seeding code creates the admin user. `SeedTestUsers()` adds test-only users via the app's DI container.

---

### Feature 6 — Docker + docker-compose

**Why it matters for Germany:** Containerization is a hard requirement in virtually all German mid/senior .NET job postings.

**`Dockerfile`** (multi-stage):
```
Stage 1 (sdk:8.0):   dotnet restore → dotnet publish -c Release
Stage 2 (aspnet:8.0): copies published output → runs as non-root user
Final image size: ~220MB
```

**`docker-compose.yml`** services:
1. `sqlserver` — SQL Server 2022 Express with persistent volume and health check
2. `api` — the application, waits for sqlserver to be healthy before starting

**Key design decisions:**
- All secrets passed via environment variables (`Jwt__Key`, `DB_PASSWORD`) — 12-Factor App
- Non-root user in Dockerfile — security best practice
- `depends_on: condition: service_healthy` — no race condition on startup
- API health check: `curl -f http://localhost:8080/health`

**Run the entire stack:**
```bash
docker-compose up --build
```

---

### Feature 7 — GitHub Actions CI/CD

**Why it matters for Germany:** German engineering culture values process discipline. A green CI badge signals professional workflow.

**File:** `.github/workflows/ci.yml`

**Triggers:** Push or PR to `main`/`master`

**Jobs:**

**`build-and-test`:**
1. `actions/checkout@v4`
2. `actions/setup-dotnet@v4` with `dotnet-version: 8.0.x`
3. NuGet cache keyed on `**/*.csproj` hash
4. `dotnet restore` → `dotnet build --configuration Release` → `dotnet test` with code coverage
5. Upload coverage XML as artifact

**`docker-build`** (runs after tests pass):
1. Build Docker image tagged with git SHA
2. Verify container starts with test environment variables

---

### Feature 8 — SignalR Real-Time Hub

**Why it matters for Germany:** Shows full-stack real-time capability. Makes the project visually demonstrable in a 5-minute interview demo.

**No extra packages needed** — SignalR is included in ASP.NET Core 8.

**New files:**
```
Hubs/INotificationClient.cs    — typed hub interface
Hubs/NotificationHub.cs        — [Authorize] typed hub
```

**`INotificationClient` interface (typed hub):**
```csharp
public interface INotificationClient
{
    Task NotificationProcessed(NotificationResponseDto notification);
    Task NotificationFailed(int notificationId, string reason);
}
```

**Hub endpoint:** `ws://localhost:8080/hubs/notifications` (requires Bearer token)

**Worker integration:**
- `NotificationWorker` injects `IHubContext<NotificationHub, INotificationClient>`
- After successful delivery: `await _hub.Clients.All.NotificationProcessed(dto)`
- After permanent failure: `await _hub.Clients.All.NotificationFailed(id, reason)`

**CORS updated for SignalR:**
```csharp
policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials(); // Required for SignalR
```

**Browser connection example (JavaScript):**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:8080/hubs/notifications", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

connection.on("NotificationProcessed", (notification) => {
    console.log("Live notification:", notification);
});

await connection.start();
```

---

## 5. Final Project Structure

```
D:/Dotnet_Project/
├── Smart_Notification_System/
│   ├── .github/
│   │   └── workflows/
│   │       └── ci.yml
│   ├── Application/
│   │   ├── Common/
│   │   │   └── Behaviors/
│   │   │       └── ValidationBehavior.cs
│   │   └── Notifications/
│   │       ├── Commands/
│   │       │   ├── CreateNotificationCommand.cs
│   │       │   ├── CreateNotificationCommandHandler.cs
│   │       │   ├── DeleteNotificationCommand.cs
│   │       │   ├── DeleteNotificationCommandHandler.cs
│   │       │   ├── UpdateNotificationCommand.cs
│   │       │   └── UpdateNotificationCommandHandler.cs
│   │       ├── Queries/
│   │       │   ├── GetNotificationByIdQuery.cs
│   │       │   ├── GetNotificationByIdQueryHandler.cs
│   │       │   ├── GetPagedNotificationsQuery.cs
│   │       │   └── GetPagedNotificationsQueryHandler.cs
│   │       ├── Validators/
│   │       │   ├── CreateNotificationCommandValidator.cs
│   │       │   └── UpdateNotificationCommandValidator.cs
│   │       └── NotificationMapper.cs
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── NotificationController.cs
│   ├── DTO/
│   │   ├── LoginRequestDto.cs
│   │   ├── NotificationRequestDto.cs
│   │   ├── NotificationResponseDto.cs
│   │   ├── PagedResponseDto.cs
│   │   ├── RefreshTokenRequestDto.cs
│   │   ├── RegisterRequestDto.cs
│   │   └── UpdateNotificationDto.cs
│   ├── Hubs/
│   │   ├── INotificationClient.cs
│   │   └── NotificationHub.cs
│   ├── Middelwares/
│   │   └── ExceptionMiddleware.cs
│   ├── Middlewares/
│   │   └── CorrelationIdMiddleware.cs
│   ├── Models/
│   │   ├── AppDBContext.cs
│   │   ├── Notification.cs
│   │   └── User.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Repositories/
│   │   ├── INotificationRepository.cs
│   │   └── NotificationRepository.cs
│   ├── Services/
│   │   └── JwtService.cs
│   ├── Workers/
│   │   └── NotificationWorker.cs
│   ├── .dockerignore
│   ├── Dockerfile
│   ├── Filters.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── docker-compose.yml
│   └── Smart_Notification_System.csproj
│
└── Smart_Notification_System.Tests/
    ├── Integration/
    │   ├── AuthApiIntegrationTests.cs
    │   ├── CustomWebApplicationFactory.cs
    │   └── NotificationApiIntegrationTests.cs
    ├── Unit/
    │   ├── Commands/
    │   │   ├── CreateNotificationCommandHandlerTests.cs
    │   │   └── DeleteNotificationCommandHandlerTests.cs
    │   ├── Services/
    │   │   └── JwtServiceTests.cs
    │   └── Workers/
    │       └── NotificationWorkerTests.cs
    └── Smart_Notification_System.Tests.csproj
```

---

## 6. API Reference

### Authentication — `/api/auth`

#### `POST /api/auth/register`
Register a new user.
```json
// Request body
{ "username": "johndoe", "password": "Secret@123", "role": "User" }

// Response 201
{ "message": "User registered successfully." }

// Response 409 (duplicate username)
{ "message": "Username 'johndoe' is already taken." }
```

#### `POST /api/auth/login`
```json
// Request body
{ "username": "admin", "password": "Admin@123" }

// Response 200
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "aGVsbG8...",
  "expiresIn": 900,
  "role": "Admin"
}
```

#### `POST /api/auth/refresh`
```json
// Request body
{ "refreshToken": "aGVsbG8..." }

// Response 200
{ "accessToken": "eyJhbGci...", "refreshToken": "bmV3...", "expiresIn": 900, "role": "Admin" }
```

#### `POST /api/auth/logout`  *(Requires Bearer token)*
```json
// Request body
{ "refreshToken": "bmV3..." }

// Response 204 No Content
```

---

### Notifications — `/api/notifications` *(All require Bearer token)*

#### `POST /api/notifications`  *(Admin only)*
```json
// Request body
{
  "message": "System maintenance tonight at 10pm",
  "type": "Email",         // Email | SMS | Push | Webhook
  "priority": "High",      // Low | Normal | High | Critical
  "scheduledAt": null      // optional ISO 8601 future datetime
}

// Response 201
{
  "id": 1,
  "message": "System maintenance tonight at 10pm",
  "type": "Email",
  "priority": "High",
  "isProcessed": false,
  "isFailed": false,
  "retryCount": 0,
  "createdAt": "2026-04-14T10:00:00Z",
  "processedAt": null,
  "scheduledAt": null
}

// Response 422 (validation error)
{
  "status": 422,
  "title": "Validation Failed",
  "errors": {
    "Priority": ["Priority must be one of: Low, Normal, High, Critical."]
  }
}
```

#### `GET /api/notifications`
```
Query params:
  page       (default: 1)
  pageSize   (default: 10, max: 100)
  type       (optional: Email | SMS | Push | Webhook)
  priority   (optional: Low | Normal | High | Critical)
  isProcessed (optional: true | false)

Response 200:
{
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

#### `GET /api/notifications/{id}`
```
Response 200: NotificationResponseDto
Response 404: { "message": "Notification 99 not found." }
```

#### `PATCH /api/notifications/{id}`  *(Admin only)*
```json
// Request body (all fields optional)
{ "message": "Updated message", "priority": "Critical" }

// Response 200: updated NotificationResponseDto
// Response 404: not found
```

#### `DELETE /api/notifications/{id}`  *(Admin only)*
```
Response 204 No Content
Response 404: not found
```

---

### Other Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Database connectivity health check |
| `GET /` | Swagger UI |
| `WS /hubs/notifications` | SignalR hub (requires Bearer token) |

---

## 7. How to Run

### Option A — Docker (Recommended, zero setup)

```bash
cd D:/Dotnet_Project/Smart_Notification_System

# Optional: set secrets in .env file
# JWT_KEY=your-strong-secret-min-32-chars
# DB_PASSWORD=YourStrong@Passw0rd

docker-compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080`
- Health: `http://localhost:8080/health`

### Option B — Local (requires SQL Server LocalDB)

1. Ensure SQL Server LocalDB is installed (comes with Visual Studio)
2. Update `appsettings.Development.json`:
   - Set a strong `Jwt:Key` (min 32 characters)
3. Run:
```bash
cd D:/Dotnet_Project/Smart_Notification_System
dotnet run
```

The database is created automatically on first run via `EnsureCreated()`.

> **Production note:** Replace `db.Database.EnsureCreated()` in `Program.cs` with migrations:
> ```bash
> dotnet ef migrations add InitialCreate
> dotnet ef database update
> ```

### Default Credentials (seeded on first run)

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin@123` | Admin |

---

## 8. How to Test

### Run All Tests
```bash
cd D:/Dotnet_Project/Smart_Notification_System.Tests
dotnet test
```

Expected: **22 tests passing, 0 failures**

### Run with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Breakdown

| Category | Count | What's Covered |
|----------|-------|----------------|
| Unit — Command Handlers | 4 | Create/Delete handler happy path + not found |
| Unit — Worker | 2 | Processes pending; no-op when empty |
| Unit — JwtService | 5 | Token format, refresh randomness, hash consistency, short-key guard |
| Integration — Auth | 5 | Login success/fail, register, duplicate, refresh token |
| Integration — Notifications | 6 | CRUD, 401/403 enforcement, 422 validation, pagination |
| **Total** | **22** | |

### Test Architecture

- **Unit tests** use `NSubstitute` to mock `INotificationRepository` — no DB, no network, fast
- **Integration tests** use `WebApplicationFactory<Program>` — spins up the real app with InMemory DB
- `CustomWebApplicationFactory` replaces SQL Server with InMemory DB for isolation
- `FluentAssertions` makes assertions read like English: `result.Should().Be(...)`

---

## 9. Technology Stack

| Category | Technology | Version |
|----------|-----------|---------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | SQL Server / LocalDB | 2022 |
| Authentication | JWT Bearer | 8.0.5 |
| Password Hashing | BCrypt.Net-Next | 4.0.3 |
| CQRS | MediatR | 12.4.1 |
| Validation | FluentValidation | 11.x |
| Logging | Serilog | 8.0.3 |
| Real-Time | SignalR (built-in) | — |
| API Docs | Swashbuckle / Swagger | 6.4.0 |
| Containerization | Docker / docker-compose | — |
| CI/CD | GitHub Actions | — |
| Test Framework | xUnit | 2.9.0 |
| Mocking | NSubstitute | 5.1.0 |
| Test Assertions | FluentAssertions | 6.12.1 |
| Integration Tests | Microsoft.AspNetCore.Mvc.Testing | 8.0.0 |

---

## 10. Key Concepts for Interviews

### CQRS (Command Query Responsibility Segregation)
**What:** Separating read operations (Queries) from write operations (Commands).
**Why:** Allows independent scaling, optimization, and testing of reads vs writes.
**In this project:** Each use case has its own `IRequest<T>` record and `IRequestHandler`. Controllers only call `_mediator.Send(...)` — they don't know how things are done.

### MediatR Pipeline Behaviors
**What:** Middleware for the MediatR pipeline — runs before/after every handler.
**In this project:** `ValidationBehavior<TRequest, TResponse>` runs all validators before the handler executes. If validation fails, the handler never runs.

### JWT Refresh Tokens
**What:** Short-lived access tokens (15min) + long-lived refresh tokens (7 days) enable stateless auth with revocation capability.
**Flow:** Login → get both tokens → use access token for API calls → when expired, call `/refresh` → get new access token + rotated refresh token → on logout, null refresh token in DB.

### Repository Pattern + Dependency Inversion
**What:** `INotificationRepository` interface decouples business logic from data access.
**Why:** Handlers depend on the interface, not the concrete SQL implementation → easy to swap DB, easy to mock in tests.

### FluentValidation vs Data Annotations
**Data Annotations:** `[Required]`, `[MaxLength]` on DTO properties — simple but limited, validation tied to the model.
**FluentValidation:** Rules in separate validator classes, can be complex, conditional, cross-field. In this project, validators are injected by the MediatR pipeline behavior — zero boilerplate in controllers.

### Serilog Structured Logging
**Why structured:** Traditional logging: `"User admin logged in"` — hard to query.
Structured: `{ "event": "UserLogin", "username": "admin", "correlationId": "abc-123" }` — queryable in ELK/Splunk.
**Correlation ID:** Every HTTP request gets a unique ID. All log lines during that request include it → full request trace across all logs.

### SignalR Typed Hubs
**What:** Instead of calling `Clients.All.SendAsync("MethodName", data)` (string-based, no compile-time check), typed hubs use an interface: `Clients.All.NotificationProcessed(dto)` → compile-time safety.

### Docker Multi-Stage Build
**Why multi-stage:** The SDK image (~700MB) is only needed to build. The runtime image (~200MB) is what runs in production. Multi-stage builds throw away the SDK after compilation.

### GitHub Actions
**Cache strategy:** NuGet packages are cached using the hash of all `.csproj` files as the key. If no `.csproj` files changed, packages are restored from cache → faster builds.

---

## Appendix — appsettings.json Reference

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartNotificationDb;..."
  },
  "Jwt": {
    "Key": "min 32 characters — use Azure Key Vault in production",
    "Issuer": "SmartNotificationSystem",
    "Audience": "SmartNotificationSystemUsers",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "NotificationWorker": {
    "IntervalSeconds": 5,
    "MaxRetryCount": 3
  },
  "RateLimit": {
    "PermitLimit": 100,
    "WindowSeconds": 60
  },
  "AllowedOrigins": ["http://localhost:3000"],
  "Serilog": { ... }
}
```

---

*Document generated: April 2026*
*Project: Smart Notification System | Platform: .NET 8 | Target Market: Germany*
