# Planning Reference

## Implementation Plan

The full multi-phase implementation plan is at:
[`../Plans/read-the-interview-assignment-mighty-bachman.md`](../Plans/read-the-interview-assignment-mighty-bachman.md)

## Per-Phase Bite-Sized Plans

Phases 1–7 from the original plan were executed as four implementation phases plus a post-completion fix pass:

| Phase | Scope | Key Deliverables | Status |
|---|---|---|---|
| 0 | Bootstrap | Repo scaffold, solution structure, tooling | ✅ Done |
| 1 | Domain model | C# domain entities, EF Core config, SQLite migrations, seed data | ✅ Done |
| 2 | API endpoints | Minimal API endpoints: slots, appointments (CRUD + status + notes), users, branches, service types; xUnit integration tests | ✅ Done |
| 3 | Frontend | React + Vite SPA: design tokens, API client (Zod schemas), IdentityContext, AppShell, BookingPage, MechanicPage, AdminPage, UI primitives; Vitest unit tests | ✅ Done |
| 4 | Polish & QA | Playwright E2E tests for all three role paths, README, PRODUCT.md, DESIGN.md, docs/prompts.md archive | ✅ Done |

## Post-Completion Fixes (May 18 2026)

| Fix | Commit |
|---|---|
| Vite proxy target `localhost` → `127.0.0.1` (Windows IPv4 issue) | `5dcbadc` |
| Frontend ID types migrated from `number` to `string` (Guid) across all schemas, pages, and tests | `e9478f1` |
| All Zod schema field names aligned to actual backend DTO fields (`startUtc`, `endUtc`, `vehicleRegistration`, `customerPhone`, `body`, `authorName`, etc.) | `fc23299` |
| Dialog CSS: add `position:fixed; inset:0; margin:auto` for viewport centering; add `:not([open]){display:none}` to prevent pre-render flash | `e3c8aed` |
| Book form: compact slot summary, two-column name/phone row, reduce notes textarea height | `e3c8aed` |
| Admin view: clickable appointment rows open read-only detail dialog | `26af5d8` |
| Work note submission: fix payload `{content}` → `{body, authorMechanicId}` matching `AddWorkNoteRequest` DTO | `26af5d8` |

## Subagent / Skill Routing

| Need | Agent / Skill |
|---|---|
| Expand phase into bite-sized tasks | `superpowers:writing-plans` + `ecc:planner` |
| TDD per unit | `superpowers:test-driven-development` + `ecc:tdd-guide` |
| Fresh isolated task execution | `superpowers:subagent-driven-development` |
| C# / EF review | `ecc:csharp-reviewer`, `ecc:database-reviewer` |
| React / TS review | `ecc:typescript-reviewer` |
| Security | `ecc:security-reviewer` |
| UI design direction | `impeccable teach/shape/craft/critique/polish/audit` |
| UI quality | `ui-ux-pro-max:ui-ux-pro-max` |
| E2E tests | `ecc:e2e-runner` |
| Docs update | `ecc:doc-updater` |
| Final verification | `superpowers:verification-before-completion` |
