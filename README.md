# AA SMR Appointment Scheduler

> Internal scheduling tool for the AA Service, Maintenance & Repair team. Replaces spreadsheets and shared calendars with a proper booking workflow for three roles: Booking Agent, Mechanic, and Admin.

## How to Run

**Prerequisites:** .NET 9 SDK, Node 18+, SQL Server LocalDB (`MSSQLLocalDB` instance).

```bash
# 1. Copy the example config and add your connection string
cp api/AaSmr.Api/appsettings.Development.example.json api/AaSmr.Api/appsettings.Development.json
# (edit the ConnectionStrings.Default value if your LocalDB instance differs)

# 2. Start the API (auto-applies migrations and seeds data on first run)
dotnet run --project api/AaSmr.Api

# 3. In a second terminal, start the frontend
npm --prefix web install
npm --prefix web run dev
```

Then open http://localhost:5173. Use the **Act As** dropdown (top-right) to switch between named seed users.

## Stack & Why

| Layer | Choice | Rationale |
|---|---|---|
| Frontend | React 18 + Vite + TypeScript | Fast HMR, rich ecosystem, clear separation from API, better iteration speed than Blazor for a UI-heavy tool |
| State / data | TanStack Query + Zod | Server-state caching, type-safe API contract, automatic cache invalidation on role switch |
| Backend | ASP.NET Core 8 Minimal API | Lightweight, idiomatic .NET 8, easy to host, good EF integration |
| ORM | EF Core 8 (SqlServer) | Code-first migrations, type-safe queries, auto-applies schema on startup |
| Database | SQL Server LocalDB | Zero-install for dev; easy to swap to full SQL Server or Azure SQL in production |
| Tests | xUnit + FluentAssertions (API), Vitest + Testing Library (UI), Playwright (E2E) | Industry standard for .NET + React; fast, watch-mode friendly |

## Build & Phase Status

| Phase | Description | Status |
|---|---|---|
| 0 | Bootstrap: repo, scaffolding, .gitignore, README | ✅ Done |
| 1 | Domain model, EF Core, migrations, seed data | ✅ Done |
| 2 | Backend API endpoints | 📋 Planned |
| 3 | Frontend scaffold, identity context, design system | 📋 Planned |
| 4 | Booking agent flow (UI) | 📋 Planned |
| 5 | Mechanic flow (UI) | 📋 Planned |
| 6 | Admin home (UI) | 📋 Planned |
| 7 | Polish, E2E tests, final verification | 📋 Planned |

## What's Done / Not Done

**Done:**
- Project scaffolding, git setup, GitHub repo, solution + web skeleton.
- Domain model: 7 entities + 2 enums with EF Core 8 configurations.
- Initial migration (auto-applies on first run via `MigrateAsync`).
- Idempotent seed: 3 branches (Dublin/Cork/Galway), 4 service types, 4 mechanics, 6 named users, 224 appointment slots across next 7 days.
- 38 passing tests (entity shapes, unique indexes, seed counts, integration startup test).

**Not done yet:**
- Backend API endpoints (Phase 2).
- Frontend (Phases 3–6).
- E2E tests and polish (Phase 7).

## Known Rough Edges

- None yet.

## AI Tools Used

- **Claude Code** (Opus 4.7 via Claude Code CLI) — primary AI assistant for planning, code generation, and code review.
- **ECC subagents** — planner, tdd-guide, csharp-reviewer, typescript-reviewer, security-reviewer, e2e-runner (dispatched from within Claude Code).
- **Impeccable skill** — UI design direction, component shaping, critique, and polish passes.
- **UI-UX-PRO-MAX skill** — frontend component-level design quality direction.

## Planning

Implementation plan: [`Plans/read-the-interview-assignment-mighty-bachman.md`](Plans/read-the-interview-assignment-mighty-bachman.md)

Per-phase bite-sized plans: [`docs/superpowers/plans/`](docs/superpowers/plans/) (generated at the start of each phase).

## Prompts

All significant prompts are archived in [`docs/prompts.md`](docs/prompts.md).

## Future Work (Out of Scope per Brief)

Items deliberately not implemented:

- **Authentication / login** — the Act-As dropdown simulates user identity; a real login system would use OAuth2/OIDC.
- **Email / SMS notifications** — on booking confirmation and status changes.
- **Rescheduling and cancellation flows** — slots are currently book-only.
- **Recurring appointments** — e.g., fleet vehicles every 6 months.
- **Payments / invoicing**.
- **Mobile-specific UI** — the app is responsive but not a mobile-first or PWA design.
- **Production config hardening** — secrets in env/Key Vault; migrations gated behind a deploy step rather than auto-applied at startup.
- **Multi-tenant branch isolation** — branches share one DB; tenant separation is a future concern.
- **Audit log** — status-change and note-edit history.
