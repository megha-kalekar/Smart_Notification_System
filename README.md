# Smart Notification System

A **production-ready REST API** built with **ASP.NET Core 8** that manages and delivers notifications through multiple channels (Email, SMS, Push, Webhook) with real-time updates via SignalR.

> Built as a portfolio project demonstrating enterprise-level .NET architecture for the German job market.

---

## Features

| Feature | Technology |
|---------|-----------|
| REST API with JWT Authentication | ASP.NET Core 8 |
| CQRS Pattern | MediatR 12 |
| Input Validation | FluentValidation |
| Real-Time Push Notifications | SignalR |
| Background Processing with Retry | BackgroundService |
| Structured Logging + Correlation IDs | Serilog |
| Rate Limiting | ASP.NET Core Rate Limiter |
| Health Checks | EF Core Health Check |
| Unit + Integration Tests | xUnit, NSubstitute, FluentAssertions |
| Containerization | Docker + docker-compose |
| CI/CD Pipeline | GitHub Actions |
| API Documentation | Swagger / OpenAPI |
| Refresh Token Auth | JWT + SHA-256 hashed tokens |
| Soft Delete + Pagination | Entity Framework Core 8 |

---

## Architecture

```
Controllers  →  MediatR  →  Handlers  →  INotificationRepository  →  SQL Server
     ↑               ↓
  JWT Auth     ValidationBehavior (FluentValidation)
     ↑
Rate Limiter
     ↑
ExceptionMiddleware + CorrelationIdMiddleware
```

**Design Patterns:** Repository, CQRS, Mediator, Pipeline, Observer (SignalR), Decorator (Filters), Soft Delete, DTO

**SOLID Principles:** All five principles applied throughout — each class has one responsibility, handlers are open for extension, all dependencies are on interfaces.

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- OR [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run Locally

```bash
git clone https://github.com/YourUsername/Smart-Notification-System.git
cd Smart-Notification-System
dotnet run --project Smart_Notification_System
```

Open **http://localhost:5116/swagger** in your browser.

Default admin credentials:
- Username: `admin`
- Password: `Admin@123`

### Run with Docker

```bash
docker-compose up --build
```

API will be available at **http://localhost:8080/swagger**

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | None | Register new user |
| POST | `/api/auth/login` | None | Get access + refresh tokens |
| POST | `/api/auth/refresh` | None | Refresh access token |
| POST | `/api/auth/logout` | JWT | Revoke refresh token |
| GET | `/api/notifications` | JWT | Get paginated notifications |
| GET | `/api/notifications/{id}` | JWT | Get notification by ID |
| POST | `/api/notifications` | JWT Admin | Create notification |
| PATCH | `/api/notifications/{id}` | JWT Admin | Update notification |
| DELETE | `/api/notifications/{id}` | JWT Admin | Soft delete notification |
| GET | `/health` | None | Health check |
| WS | `/hubs/notifications` | JWT | SignalR real-time hub |

---

## Running Tests

```bash
dotnet test
```

```bash
# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test coverage includes:**
- Unit tests: Command handlers, background worker retry logic, JWT service
- Integration tests: Full HTTP stack with WebApplicationFactory and in-memory database

---

## Project Structure

```
Smart_Notification_System/
├── Application/
│   ├── Common/Behaviors/     # MediatR pipeline (ValidationBehavior)
│   └── Notifications/
│       ├── Commands/         # Create, Update, Delete handlers
│       ├── Queries/          # GetById, GetPaged handlers
│       └── Validators/       # FluentValidation rules
├── Controllers/              # Thin HTTP controllers (MediatR only)
├── DTO/                      # Request/Response data shapes
├── Hubs/                     # SignalR typed hub + interface
├── Middlewares/              # ExceptionMiddleware, CorrelationIdMiddleware
├── Models/                   # EF Core entities + DbContext
├── Repositories/             # INotificationRepository + implementation
├── Services/                 # JwtService
├── Workers/                  # Background notification processor
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/ci.yml

Smart_Notification_System.Tests/
├── Unit/                     # Isolated tests with mocks
└── Integration/              # Full HTTP stack tests
```

---

## Configuration

Key settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",
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
  }
}
```

For Docker, secrets are passed via environment variables — never hardcoded.

---

## CI/CD

GitHub Actions pipeline runs on every push to `main`:
1. Restore NuGet packages (cached)
2. Build in Release mode
3. Run all tests
4. Build Docker image

---

## Tech Stack

- **Runtime:** .NET 8 / ASP.NET Core 8
- **Database:** SQL Server (EF Core 8, Code-First)
- **Auth:** JWT Bearer + BCrypt.Net + Refresh Tokens
- **Messaging:** MediatR 12 (CQRS)
- **Validation:** FluentValidation
- **Real-Time:** SignalR
- **Logging:** Serilog (Console + Rolling File)
- **Testing:** xUnit, NSubstitute, FluentAssertions, WebApplicationFactory
- **Container:** Docker, docker-compose
- **CI/CD:** GitHub Actions

---

## License

MIT
