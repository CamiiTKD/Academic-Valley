# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the API (requires PostgreSQL running)
dotnet run --project src/AcademicPlanner.API

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~CalcularProgresoHandlerTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~Handle_SinMaterias_RetornaTodoEnCero"

# Build the solution
dotnet build

# Add a new EF migration
dotnet ef migrations add <MigrationName> --project src/AcademicPlanner.Infrastructure --startup-project src/AcademicPlanner.API

# Apply migrations manually
dotnet ef database update --project src/AcademicPlanner.Infrastructure --startup-project src/AcademicPlanner.API

# Start the full stack (API + PostgreSQL) with Docker
docker-compose up
```

## Database

- PostgreSQL 16 via Npgsql + EF Core.
- **Local dev**: connect on port `5433` (see `appsettings.json`). Password: `MiClaveSuperSegura123`.
- **Docker**: port `5432`, password `postgres` (set via env vars in `docker-compose.yml`).
- Migrations run automatically on startup in `Development` environment (`db.Database.MigrateAsync()`).
- Enums are stored as strings in the database (configured in `MateriaConfiguration`).
- `Materia.Codigo` has a unique index (`IX_Materias_Codigo`).
- Swagger UI is served at the root (`/`) in Development.

## Architecture

Clean Architecture with four projects:

```
Domain → Application → Infrastructure
                  ↑
                 API
```

**Domain** (`AcademicPlanner.Domain`): Entities and enums, no dependencies. Domain logic lives exclusively on the entity (e.g., `Materia.Crear`, `Materia.ActualizarEstado`). All entity properties have `private set` — mutations go through domain methods only. `BaseEntity` provides a `Guid Id` auto-generated on construction.

**Application** (`AcademicPlanner.Application`): MediatR-based CQRS. Each feature lives under `Features/<Feature>/Commands|Queries/<Name>/`, with three files: `Command/Query` (record implementing `IRequest`), `Handler`, and `Validator`. FluentValidation runs as a MediatR pipeline behavior (`ValidationBehavior`) — validation exceptions bubble up automatically before any handler executes. Repository interfaces (`IMateriaRepository`, `IEvaluacionRepository`, `IHorarioCursadaRepository`, `IUnitOfWork`) are defined here; implemented in Infrastructure.

**Infrastructure** (`AcademicPlanner.Infrastructure`): EF Core `AcademicPlannerDbContext` directly implements `IUnitOfWork`. Entity configurations are in `Persistence/Configurations/` using `IEntityTypeConfiguration<T>`, registered via `ApplyConfigurationsFromAssembly`. Repositories inject `AcademicPlannerDbContext`.

**API** (`AcademicPlanner.API`): Thin controllers that only call `mediator.Send(command)`. Enums serialize as strings in JSON responses. XML doc comments on controller actions are included in Swagger. CORS is configured for React frontends on ports 3000 and 5173. `public partial class Program` is exposed for future integration test `WebApplicationFactory` usage.

**Tests** (`AcademicPlanner.Tests`): xUnit + FluentAssertions + NSubstitute. Tests only cover Application layer handlers; repositories are mocked with NSubstitute. No integration tests yet (Infrastructure has no test project).

## Serialization gotchas

These are non-obvious and have caused bugs before — check here first before touching enums or date/time types.

- **All enums serialize as strings** via a global `JsonStringEnumConverter` registered in `Program.cs`. This includes `DayOfWeek`. Frontend must use string keys (`"Monday"`, `"Tuesday"`, etc.), never numeric indices. Wrong: `DIAS[h.diaSemana]` (treats string as array index → undefined). Right: `DIA_LABEL[h.diaSemana]` (object lookup by key).
- **`TimeOnly`** serializes as `"HH:mm:ss"`. Frontend should slice: `t.slice(0, 5)` to display `"HH:mm"`.
- **`DateOnly`** serializes as `"YYYY-MM-DD"`. Always parse with `split('-')` and construct the date manually — never pass the ISO string directly to `new Date()` (UTC midnight shifts the date by one day in local timezones).

## Agenda Semanal feature

- `GET /api/Agenda/semanal` returns `IReadOnlyList<DiaAgendaDto>` — always **7 entries in Mon→Sun order**, even if a day has no horarios or evaluaciones. Frontend can rely on all 7 days always being present.
- `DiaAgendaDto` contains `DiaSemana` (string, e.g. `"Monday"`), `Horarios` (list of `HorarioMateriaDto`), and `Alertas` (list of `AlertaEvaluacionDto` for evaluaciones within the next 7 days from today).
- `GetAgendaSemanalHandler` calls `IMateriaRepository.GetCursandoWithHorariosAndEvaluacionesAsync` which eager-loads both `.Include(m => m.Horarios)` and `.Include(m => m.Evaluaciones)` in a single query (use this method when you need both; avoid calling separate repos).
- `AgendaModal` (frontend) shows Sat/Sun columns only when they have content; always shows Mon–Fri.

## Key domain rules

- `EstadoMateria` states: `Pendiente → Cursando → Regular → Aprobada`.
- `NotaFinal` is required (and must be 1–10) when marking a `Materia` as `Aprobada`; must be null when marking as `Pendiente`.
- `Materia.Correlativas` is a self-referencing many-to-many stored in a `MateriasCorrelativas` join table. A materia cannot be its own correlative.
- `Evaluacion.Nota` must be between 1 and 10.
- `CalcularProgresoHandler` computes two averages: `PromedioSinAplazos` (only `Aprobada` materias) and `PromedioConAplazos` (all materias with any `NotaFinal`).
