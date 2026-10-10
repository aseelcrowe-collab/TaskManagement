# Task Management API

ASP.NET Core Web API for managing tasks, backed by **EF Core + SQL Server**.

## Features

- Full task CRUD (`GET /api/tasks`, `GET /api/tasks/{id}`, `POST`, `PUT`, `PATCH …/status`, `DELETE`)
- Filtering by status / priority, free-text search, overdue-only, due-date range
- Sorting (`createdAt`, `dueDate`, `priority`, `title`, `status`) + pagination with metadata
- Aggregate counts via `GET /api/tasks/stats`
- Validation with data annotations (automatic `400` responses)
- Global exception handling returning RFC 7807 problem details (`404` for missing tasks)
- Swagger UI (Swashbuckle) at `/swagger`
- EF Core code-first migrations against SQL Server
- xUnit service-layer tests (EF Core InMemory provider)
- Docker support (`Dockerfile` + `docker-compose.yml` with SQL Server 2022)

## Project structure

```text
TaskManagement/
├── TaskManagement.Api/
│   ├── Controllers/TasksController.cs  # HTTP endpoints
│   ├── Data/AppDbContext.cs            # EF Core context + mappings
│   ├── DTOs/                           # Create/Update/Response/Query/Stats shapes
│   ├── Models/TaskItem.cs              # Task entity + TaskStatus/TaskPriority enums
│   ├── Services/                       # ITaskService + TaskService (business logic)
│   ├── Middleware/ExceptionHandlingMiddleware.cs
│   ├── Program.cs                      # DI, Swagger, CORS, migrate + seed
│   ├── appsettings.json                # DefaultConnection (LocalDB)
│   └── Dockerfile
├── TaskManagement.Tests/               # xUnit tests (TaskServiceTests)
├── docker-compose.yml                  # API + SQL Server 2022
└── README.md
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (pick one):
  - **Docker** (recommended): SQL Server 2022 via `docker-compose.yml`, or
  - **Local**: LocalDB / SQL Express / full SQL Server on the dev machine

## Run with Docker (recommended)

```bash
docker compose up --build
```

- API: <http://localhost:8080> (Swagger: <http://localhost:8080/swagger>)
- SQL Server: `localhost,1433` (`sa` / `YourStrong!Pass123` — change it in `docker-compose.yml`)

The API applies EF Core migrations automatically at startup.

## Run locally (no Docker)

1. Make sure a SQL Server instance is reachable, then point the API at it:

   ```bash
   # Example for a local SQL Express instance:
   dotnet user-secrets init --project TaskManagement.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "Server=.\\SQLEXPRESS;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true" \
     --project TaskManagement.Api
   ```

   Default (no override): `Server=(localdb)\mssqllocaldb;Database=TaskManagementDb;...` (see `appsettings.json`).

2. Restore, create the database and run:

   ```bash
   dotnet restore
   dotnet ef database update --project TaskManagement.Api   # needs: dotnet tool install -g dotnet-ef
   dotnet run --project TaskManagement.Api
   ```

   Swagger: <https://localhost:7000/swagger> (or the HTTP profile URL printed in the console).
   In `Development`, three sample tasks are seeded on first run.

## API reference

| Method | Route | Description |
| ------ | ----- | ----------- |
| GET | `/api/tasks` | List tasks (filter / sort / paginate) |
| GET | `/api/tasks/stats` | `{ total, todo, inProgress, done, overdue }` |
| GET | `/api/tasks/{id}` | Single task (`404` if missing) |
| POST | `/api/tasks` | Create task → `201 Created` + `Location` header |
| PUT | `/api/tasks/{id}` | Fully replace a task |
| PATCH | `/api/tasks/{id}/status` | Change only the status |
| DELETE | `/api/tasks/{id}` | Delete → `204 No Content` |

Query parameters for `GET /api/tasks`:

| Parameter | Example | Description |
| --------- | ------- | ----------- |
| `status` | `?status=Todo` | `Todo`, `InProgress`, `Done` |
| `priority` | `?priority=High` | `Low`, `Medium`, `High` |
| `search` | `?search=report` | Matches title + description (case-insensitive) |
| `overdueOnly` | `?overdueOnly=true` | Past due date and not `Done` |
| `dueBefore` / `dueAfter` | `?dueBefore=2026-12-01` | Due-date range |
| `sortBy` | `?sortBy=dueDate` | `createdAt` (default), `dueDate`, `priority`, `title`, `status` |
| `sortOrder` | `?sortOrder=asc` | `asc` or `desc` (default) |
| `page` / `pageSize` | `?page=2&pageSize=20` | Defaults `1` / `10` (max `100`) |

Example — create a task:

```bash
curl -X POST http://localhost:8080/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Write documentation","priority":"High","dueDate":"2026-11-01T00:00:00Z"}'
```

Example — paged, filtered list:

```bash
curl "http://localhost:8080/api/tasks?status=Todo&sortBy=dueDate&sortOrder=asc&page=1&pageSize=10"
```

## Configuration

| Setting | Default | Notes |
| ------- | ------- | ----- |
| `ConnectionStrings:DefaultConnection` | LocalDB (`appsettings.json`) | Override via env var `ConnectionStrings__DefaultConnection` |
| `ASPNETCORE_ENVIRONMENT` | `Development` (launch profile) | Seed data only runs in `Development` |

## Tests

```bash
dotnet test
```

Service-layer tests run against the EF Core InMemory provider — no SQL Server needed.

## Migrations

```bash
dotnet tool install -g dotnet-ef
dotnet ef migrations add <Name> --project TaskManagement.Api
dotnet ef database update --project TaskManagement.Api
```

Migrations are applied automatically at startup (`db.Database.Migrate()`).
