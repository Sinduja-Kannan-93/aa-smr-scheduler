# Phase 1 — Domain Model, EF Core, Migrations, Seed: Bite-Sized TDD Plan

## Logical Units (8 commits)

| # | Commit message | Files |
|---|---|---|
| 1 | `chore: add EF Core, InMemory, FluentAssertions, and Mvc.Testing packages` | *.csproj |
| 2 | `feat: add Phase 1 domain entities and enums` | Domain/*.cs + EntityShapeTests.cs |
| 3 | `feat: add AppDbContext and per-entity EF Core configurations` | Data/*.cs + tests |
| 4 | `feat: add Initial EF Core migration for Phase 1 schema` | Migrations/*.cs + MigrationFilesTests.cs |
| 5 | `feat: add idempotent DbInitializer seed (branches, service types, mechanics, users, 224 slots)` | DbInitializer.cs + DbInitializerTests.cs |
| 6 | `feat: register AppDbContext and auto-migrate/seed at startup` | Program.cs + StartupSeedIntegrationTests.cs |
| 7 | `test: add SQLite-backed unique constraint enforcement tests` | UniqueConstraintEnforcementTests.cs |
| 8 | `test: add seed idempotency and count assertion tests` | SeedCountAssertionTests.cs |

## Key Constraints
- Appointment → AppointmentSlot: NoAction
- Appointment → WorkNote: Cascade
- All other FKs: Restrict or SetNull
- Seed: 3 branches, 4 service types, 4 mechanics, 6 users, 224 slots (4×7×8)
- Startup guard: IsRelational() before MigrateAsync
- Program.cs: public partial class Program for WebApplicationFactory
