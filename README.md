# Task Management System

A RESTful API for managing tasks with JWT authentication and user-scoped data isolation. Built with .NET 8, Vertical Slice Architecture, and the REPR (Request-Endpoint-Response) pattern.

## Table of Contents

- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Docker](#docker)
- [Testing](#testing)
- [Repository Standards](#repository-standards)
- [Future Improvements](#future-improvements)

## Architecture

### Patterns and Principles

| Pattern | Purpose |
|---------|---------|
| **Vertical Slice Architecture** | Features organized by capability, not by technical layer |
| **REPR** | Request-Endpoint-Response — each endpoint is a self-contained slice |
| **CQRS** | Commands and queries separated via MediatR |
| **Result Pattern** | `Result<T>` for explicit error handling without exceptions |
| **Validation Pipeline** | FluentValidation integrated as a MediatR pipeline behavior |
| **Minimal API** | Lightweight endpoint definitions with `RouteGroupBuilder` extensions |

### Dependency Flow

```
Api  -->  Features  -->  Domain
 |            |
 +------> Infrastructure
```

- **Domain** — Entities (`TaskItem`, `User`), enums (`Priority`), and the `Result<T>`/`Error` types. Zero external dependencies.
- **Infrastructure** — EF Core persistence (`AppDbContext`, configurations, migrations), authentication services (`IPasswordHasher`, `IJwtTokenService`), and SQLite provider.
- **Features** — Vertical slices grouped by feature. Each slice contains its own request, endpoint, handler, and validator. Shared concerns (`TaskResponse`, `TaskMappings`, `ValidationBehavior`, `ResultExtensions`) live under `Common/`.
- **Api** — Composition root. Configures DI, middleware pipeline, and maps endpoint groups. Contains `GlobalExceptionHandler` for unhandled exceptions.

### Error Handling

All handlers return `Result<T>`. The `ResultExtensions.ToProblem()` method maps error codes to HTTP status codes:

| Error Code | HTTP Status |
|------------|-------------|
| `NotFound` | 404 |
| `Validation` | 400 |
| `Conflict` | 409 |
| `Unauthorized` | 401 |
| _(other)_ | 500 |

Responses follow the [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) `ProblemDetails` format.

## Project Structure

```
TaskManagementSystem/
├── TaskManagementSystem.Api/              # Composition root
│   ├── Middleware/
│   │   └── GlobalExceptionHandler.cs
│   ├── AuthEndpoints.cs                   # POST /api/auth/*
│   ├── TasksEndpoints.cs                  # /api/tasks/* (requires auth)
│   ├── Program.cs
│   └── appsettings.json
│
├── TaskManagementSystem.Features/         # Vertical slices
│   ├── Auth/
│   │   ├── Common/
│   │   │   └── AuthResponse.cs
│   │   ├── Register/
│   │   │   ├── Register.Request.cs
│   │   │   ├── Register.Endpoint.cs
│   │   │   ├── Register.Handler.cs
│   │   │   └── Register.Validator.cs
│   │   └── Login/
│   │       ├── Login.Request.cs
│   │       ├── Login.Endpoint.cs
│   │       ├── Login.Handler.cs
│   │       └── Login.Validator.cs
│   ├── Tasks/
│   │   ├── Common/
│   │   │   ├── TaskResponse.cs
│   │   │   ├── TaskMappings.cs
│   │   │   └── TaskStatusFilter.cs
│   │   ├── Create/
│   │   ├── GetById/
│   │   ├── List/
│   │   ├── Update/
│   │   ├── SetCompletion/
│   │   └── Delete/
│   └── Common/
│       ├── ValidationBehavior.cs
│       └── ResultExtensions.cs
│
├── TaskManagementSystem.Domain/           # Core domain
│   ├── Tasks/
│   │   ├── TaskItem.cs
│   │   └── Priority.cs                   # Low, Medium, High
│   ├── Users/
│   │   └── User.cs
│   └── Common/
│       ├── Result.cs
│       └── Error.cs
│
├── TaskManagementSystem.Infrastructure/   # Persistence & auth services
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── TaskItemConfiguration.cs
│   │   │   └── UserConfiguration.cs
│   │   └── Migrations/
│   └── Auth/
│       ├── IPasswordHasher.cs
│       ├── PasswordHasher.cs             # BCrypt
│       ├── IJwtTokenService.cs
│       ├── JwtTokenService.cs
│       └── JwtSettings.cs
│
├── TaskManagementSystem.Tests/            # xUnit tests
│   ├── Auth/Handlers/
│   │   └── AuthHandlerTests.cs
│   └── Tasks/
│       ├── Handlers/TaskHandlerTests.cs
│       ├── Domain/
│       │   ├── TaskItemTests.cs
│       │   └── ResultTests.cs
│       └── Api/TasksControllerTests.cs
│
├── Dockerfile
├── .dockerignore
└── TaskManagementSystem.sln
```

## API Documentation

### Authentication

Auth endpoints do not require a token.

#### Register

```
POST /api/auth/register
```

**Request:**

```json
{
  "email": "user@example.com",
  "password": "securepassword"
}
```

**Response (200):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com"
}
```

**Validation rules:** Email must be a valid email address. Password must be 8-128 characters.

#### Login

```
POST /api/auth/login
```

**Request:**

```json
{
  "email": "user@example.com",
  "password": "securepassword"
}
```

**Response (200):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com"
}
```

### Tasks

All task endpoints require a `Bearer` token in the `Authorization` header. Tasks are scoped to the authenticated user — users can only access their own tasks.

#### Create Task

```
POST /api/tasks
```

**Request:**

```json
{
  "title": "Finish report",
  "description": "Q4 summary",
  "priority": "High",
  "dueDate": "2026-03-01T00:00:00"
}
```

**Response (201):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Finish report",
  "description": "Q4 summary",
  "isCompleted": false,
  "priority": "High",
  "dueDate": "2026-03-01T00:00:00",
  "createdAt": "2026-02-07T10:00:00Z",
  "updatedAt": "2026-02-07T10:00:00Z"
}
```

**Validation rules:** Title is required and must be 1-200 characters. Description is optional, max 2000 characters. Priority must be `Low`, `Medium`, or `High`.

#### Get Task by ID

```
GET /api/tasks/{id}
```

**Response (200):** Same shape as the create response.

#### List Tasks

```
GET /api/tasks?status={filter}
```

**Query parameters:**

| Parameter | Type | Default | Values |
|-----------|------|---------|--------|
| `status` | string | `All` | `All`, `Active`, `Completed` |

**Response (200):** Array of task objects.

#### Update Task

```
PUT /api/tasks/{id}
```

**Request:**

```json
{
  "title": "Updated title",
  "description": "Updated description",
  "priority": "Low",
  "dueDate": "2026-04-01T00:00:00"
}
```

**Response (200):** Updated task object.

#### Set Completion

```
PATCH /api/tasks/{id}/complete
```

**Request:**

```json
{
  "isCompleted": true
}
```

**Response (200):** Updated task object.

#### Delete Task

```
DELETE /api/tasks/{id}
```

**Response:** `204 No Content`

### Error Responses

All errors return `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Task with ID '...' was not found."
}
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run Locally

```bash
# Clone the repository
git clone <repository-url>
cd TaskManagementSystem

# Restore dependencies
dotnet restore

# Run the API (migrations apply automatically in Development)
dotnet run --project TaskManagementSystem.Api
```

The API starts at `http://localhost:5196`. Swagger UI is available at `http://localhost:5196/swagger`.

### Workflow

1. **Register** — `POST /api/auth/register` with email and password.
2. **Copy the token** from the response.
3. **Authorize in Swagger** — Click the "Authorize" button, paste the token.
4. **Use task endpoints** — All CRUD operations are now available.

## Configuration

Configuration is loaded from `appsettings.json` and environment variables. In Development, missing values fall back to safe defaults with a console warning. In Production, missing required values throw on startup.

| Key | Required in Prod | Dev Default | Description |
|-----|------------------|-------------|-------------|
| `ConnectionStrings:DefaultConnection` | Yes | `Data Source=taskmanagement.db` | SQLite connection string |
| `Cors:AllowedOrigin` | Yes | `http://localhost:5173` | Allowed CORS origin |
| `Jwt:Secret` | Yes | Dev-only key (logged) | HMAC-SHA256 signing key (min 32 chars) |
| `Jwt:Issuer` | No | `TaskManagementSystem` | JWT issuer claim |
| `Jwt:Audience` | No | `TaskManagementSystem` | JWT audience claim |
| `Jwt:ExpirationInMinutes` | No | `60` | Token lifetime |

## Docker

### Build and Run

```bash
# Build the image
docker build -t task-management-api .

# Run the container
docker run -d --name task-api -p 8080:8080 task-management-api
```

The API is accessible at `http://localhost:8080`.

### Persist Data

Mount a volume so the SQLite database survives container restarts:

```bash
docker run -d --name task-api -p 8080:8080 \
  -v task-data:/app/data \
  task-management-api
```

### Production Configuration

Pass required configuration via environment variables:

```bash
docker run -d --name task-api -p 8080:8080 \
  -v task-data:/app/data \
  -e Jwt__Secret="your-production-secret-min-32-characters" \
  -e Cors__AllowedOrigin="https://yourdomain.com" \
  task-management-api
```

### Dockerfile Stages

| Stage | Purpose |
|-------|---------|
| `build` | Restores packages and compiles in Release mode |
| `test` | Runs all tests (optional — not referenced by the runtime stage) |
| `publish` | Produces a trimmed publish output |
| `runtime` | Minimal `aspnet:8.0` image, non-root user, exposes port 8080 |

To include the test stage in your build:

```bash
docker build --target test -t task-management-api-test .
```

## Testing

### Run Tests

```bash
dotnet test
```

### Test Coverage

62 tests across 5 test classes:

| Test Class | Tests | Scope |
|------------|-------|-------|
| `AuthHandlerTests` | 14 | Register/login handlers and validators |
| `TaskHandlerTests` | 26 | All 6 task handlers and user-scoping isolation |
| `ResultTests` | 9 | `Result<T>` and `Error` factory methods |
| `TaskItemTests` | 7 | Domain entity behavior (`MarkComplete`, `MarkIncomplete`) |
| `TasksControllerTests` | 6 | `ResultExtensions.ToProblem()` HTTP status mapping |

### Tech Stack

| Package | Purpose |
|---------|---------|
| xUnit | Test framework |
| FluentAssertions | Assertion library |
| EF Core InMemory | In-memory database for handler tests |

## Repository Standards

### Commit Convention

```
type(TMS-###): description
```

**Types:** `feature`, `fix`, `refactor`, `chore`, `test`, `docs`

### Branch Strategy

Development happens on `main`. Feature branches follow `feature/TMS-###-description`.

## Future Improvements

- Pagination and sorting for task lists
- Refresh tokens and token revocation
- Role-based authorization
- PostgreSQL / SQL Server provider for production
- Structured logging with Serilog
- Health check endpoints
- Rate limiting
- CI/CD pipeline (GitHub Actions)
