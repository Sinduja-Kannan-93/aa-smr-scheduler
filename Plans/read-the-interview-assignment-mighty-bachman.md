# AA SMR Appointment Scheduler — Multi-Phase Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILLS per phase: `superpowers:writing-plans` (to expand each phase into bite-sized tasks before coding), `superpowers:test-driven-development` (RED→GREEN→REFACTOR per logical unit), `superpowers:subagent-driven-development` (dispatch one fresh subagent per task), `superpowers:requesting-code-review` and `superpowers:receiving-code-review` between implementation and approval, `superpowers:verification-before-completion` before declaring a phase done.

**Goal:** Build a working public-repo MVP of an internal SMR (Service, Maintenance & Repair) appointment scheduler for the AA brief, with three role views (Booking Agent / Mechanic / Admin), realistic seed data, and EF Core self-applying migrations on first run.

**Architecture:** React (Vite + TypeScript) SPA → ASP.NET Core 8 Web API → EF Core 8 → SQL Server LocalDB. Single git repo; React in `web/`, .NET in `api/` (solution + projects), plan/prompts/docs in `docs/`. View switching is driven by an `Act As` dropdown bound to **named seed users** (not role types) backed by a small client-side identity context that resets all server-side query caches and route state on change.

**Tech Stack:**
- Backend: .NET 8 Web API, EF Core 8 (SqlServer provider), xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing (`WebApplicationFactory`), Microsoft.EntityFrameworkCore.InMemory (unit) and LocalDB (integration).
- Frontend: Vite + React 18 + TypeScript, React Router 6, TanStack Query, Zod, Vitest + Testing Library, Playwright (E2E smoke).
- Tooling: dotnet CLI 9.0.304, Node 24.13, npm 11.8, gh CLI 2.92, git 2.32.

---

## Context

The AA Service, Maintenance & Repair team currently coordinates customer vehicle bookings across spreadsheets and shared calendars, which causes double-bookings, missed jobs, and notes lost between mechanics. The interview brief asks for a minimum viable internal tool to replace that process, with three flows (Customer/Booking-Agent booking, Mechanic appointment management, Admin daily schedule overview), self-applying EF migrations, seeded data so the app is usable on first run, and a public GitHub repo with descriptive commits showing how the work evolved.

The plan is multi-phase with explicit user approval gates per phase because the user has stipulated: no auto-commit, no auto-push, README + prompts archive must be updated between phases, separate descriptive commits per logical unit of work, TDD throughout, and use of `ui-ux-pro-max` and `impeccable` skills for UI quality. The plan reuses ECC subagents (planner, tdd-guide, csharp-reviewer, typescript-reviewer, code-reviewer, security-reviewer, e2e-runner, doc-updater, build-error-resolver) and superpowers skills (writing-plans, test-driven-development, subagent-driven-development, requesting-code-review, receiving-code-review, verification-before-completion) rather than ad-hoc implementation.

---

## Pre-Captured Decisions

| Decision | Value |
|---|---|
| Connection string | `Server=(localdb)\MSSQLLocalDB;Database=AaSmrScheduler;Trusted_Connection=True;TrustServerCertificate=True` |
| Connection string location | `api/AaSmr.Api/appsettings.Development.json` (gitignored) — committed `appsettings.Development.example.json` documents the shape |
| GitHub repo | `Sinduja-Kannan-93/aa-smr-scheduler` (public) |
| Working tree root | `D:\SindujaKannan_Docs\AA_Interview_Assignment\AA_Internview_Claude_Code\` (PDF stays in repo root but untracked via `.gitignore`) |
| Impeccable flow | Full: `teach` → `shape` → `craft` → `polish` (with `critique` and `audit` gates) |
| Visual reference | https://www.theaa.ie/ — yellow (#FFCC00 family) + near-black neutral, motoring-services tone, clean, confident, not flashy |
| Repo branching | Single `main` branch; small descriptive commits per logical unit (one commit ≠ one phase) |
| Approval gates | After every phase: implementation → review → verification → **STOP for user approval** → commits → README + prompts update → next phase |

---

## Hard Rules Enforced Across All Phases

1. **No commit without user approval.** Stage diffs and present `git status` + `git diff --stat` summary; wait for explicit "approved" or "commit it" before `git commit`. Never `git push` without a separate explicit instruction.
2. **No phase advance** until: (a) all tests green, (b) build green, (c) app launches with `dotnet run` + `npm run dev` on a fresh clone, (d) code review feedback addressed, (e) README + `docs/prompts.md` updated, (f) commits landed.
3. **TDD always.** Failing test first, watch it fail, minimal implementation, watch it pass, refactor. Coverage target: ≥80% on backend services and React components.
4. **Many small commits.** One commit per logical unit (e.g., "add Branch entity + migration", "add slot availability query", "wire Act-As dropdown to user context"). Never a single "Phase N done" commit.
5. **`appsettings.Development.json` is gitignored.** Connection string lives only locally. A committed `appsettings.Development.example.json` documents the shape with placeholder values.
6. **EF migrations auto-apply on app startup** via `await db.Database.MigrateAsync()` in `Program.cs` (Development only — Production left as future work).
7. **View-switch reset.** Changing the Act-As selection must (a) invalidate the TanStack Query cache, (b) navigate to that role's home route, (c) clear any in-page filter/selection state. A dedicated React context (`IdentityContext`) owns this; every page reads its identity from context, never from URL or local state.
8. **Out-of-scope items are documented as future work in README.md**, not implemented. The brief's "out of scope" list (auth/login, email/SMS, rescheduling, recurring appointments, payments, mobile-specific UI) plus anything we deliberately defer goes into the README's "Future work" section before each phase closes.
9. **Save prompts.** Every meaningful user prompt and subagent prompt for this project is appended to `docs/prompts.md` at the end of each phase, with phase + timestamp + purpose.
10. **`AA_Coding_Interview_Assignment.pdf` is gitignored** so the brief PDF is not redistributed in the public repo.

---

## Repository Layout (final state)

```
aa-smr-scheduler/                       (repo root = current working folder)
  .gitignore                            ignores appsettings.Development.json, bin/, obj/, node_modules/, .vs/, *.user, PDF
  README.md                             stack, run command, status, what's done/not, future work, AI usage
  docs/
    prompts.md                          prompts archive (appended per phase)
    planning.md                         link back to this plan file + phase outcomes
    adr/                                optional architecture decision records
  api/
    AaSmr.sln
    AaSmr.Api/                          ASP.NET Core 8 Web API
      Program.cs
      appsettings.json
      appsettings.Development.example.json   committed template
      appsettings.Development.json      GITIGNORED, real conn string
      Domain/                           Branch, ServiceType, Mechanic, User, AppointmentSlot, Appointment, WorkNote
      Data/                             AppDbContext, configurations, DbInitializer (seed), Migrations/
      Features/
        Users/                          GET /api/users (named seed users for Act-As)
        Branches/
        ServiceTypes/
        Slots/                          list available, filter
        Appointments/                   book, today, mine, detail, notes, status
      Shared/                           ApiResponse<T> envelope, exception middleware
    AaSmr.Api.Tests/                    xUnit + FluentAssertions + WebApplicationFactory
  web/
    package.json
    vite.config.ts
    tsconfig.json
    index.html
    PRODUCT.md                          produced by impeccable teach
    DESIGN.md                           produced by impeccable teach / document
    src/
      main.tsx
      App.tsx
      routes/                           BookingAgentHome, BookAppointment, Confirmation, MechanicToday, AppointmentDetail, AdminHome
      features/
        identity/                       IdentityContext, ActAsDropdown, useIdentity
        slots/                          SlotsList, SlotFilterBar
        booking/                        BookingForm, useBookSlot
        appointments/                   AppointmentList, AppointmentDetail, StatusStepper, WorkNoteList
        admin/                          TodayAcrossMechanics
      ui/                               Button, Input, Select, Card, Badge, Dialog, EmptyState (Impeccable-shaped)
      lib/                              apiClient (Zod), formatters, brand tokens
      styles/                           tokens.css, typography.css, global.css
    tests/                              Vitest unit/component
    e2e/                                Playwright smoke
  docker-compose.yml                    optional; only if Phase 7 budget allows
```

---

## Subagent + Skill Routing Per Phase

| Need | Skill / Subagent |
|---|---|
| Expand a phase into bite-sized tasks before coding | `superpowers:writing-plans` (this skill) + `ecc:planner` subagent |
| Write failing tests first | `superpowers:test-driven-development` + `ecc:tdd-guide` subagent |
| Implement tasks one at a time, fresh context each | `superpowers:subagent-driven-development` |
| Review C# / EF / API code | `ecc:csharp-reviewer`, `ecc:database-reviewer` |
| Review React / TS code | `ecc:typescript-reviewer` |
| Security pass on any input-handling endpoint | `ecc:security-reviewer` |
| Fix build / TS / EF errors quickly | `ecc:build-error-resolver` |
| Establish brand/product context from theaa.ie | `impeccable teach` (one-shot, writes PRODUCT.md + DESIGN.md) |
| Plan a UI surface before coding | `impeccable shape` |
| Build a UI surface | `impeccable craft` + `ui-ux-pro-max:ui-ux-pro-max` |
| Review a UI surface | `impeccable critique` |
| Final polish | `impeccable polish` + `impeccable audit` (a11y, perf, responsive) |
| Playwright E2E generation | `ecc:e2e-runner` |
| Update README + codemaps after each phase | `ecc:doc-updater` |
| Final verification before claiming phase done | `superpowers:verification-before-completion` |
| Request + receive code review | `superpowers:requesting-code-review`, `superpowers:receiving-code-review` |

---

## Phase Template (applied to every phase)

Every phase follows this loop. Never skip steps.

1. **Expand:** Invoke `superpowers:writing-plans` (or `ecc:planner` subagent) to expand the phase's "Scope" section below into a bite-sized task plan with TDD steps and exact file paths. Save to `docs/superpowers/plans/<phase-N>-<feature>.md` inside the repo.
2. **Execute (TDD per unit):** For each unit:
   1. Failing test (RED).
   2. Run test, confirm it fails for the expected reason.
   3. Minimal implementation (GREEN).
   4. Run test, confirm pass.
   5. Refactor if needed; tests stay green.
   6. **Stage the diff. Stop. Show `git status` + `git diff --stat`. Wait for user approval.**
   7. On approval, `git commit -m "<type>: <descriptive>"` (one logical unit).
3. **Review:** Invoke `superpowers:requesting-code-review` → run language-appropriate reviewer subagent → apply `superpowers:receiving-code-review` to triage feedback → address CRITICAL/HIGH issues with the same TDD + approval + commit loop.
4. **Verify:** Invoke `superpowers:verification-before-completion`. Concretely: backend `dotnet test` green, frontend `npm test` green, `dotnet build` warning-free, `npm run build` clean, app starts (`dotnet run` + `npm run dev`) on a fresh terminal, golden path manually exercised in the browser.
5. **Document:** Update `README.md` (status table for this phase moves from `Planned` to `Done` with one-line summary), append phase prompts to `docs/prompts.md`, optionally run `ecc:doc-updater` for codemaps.
6. **Approval gate:** Present a phase-summary message with file-change list, test results, screenshots (UI phases), and a "phase N complete — approve to proceed to phase N+1?" prompt. **Do not start phase N+1 until the user types approval.**

---

## Phase 0 — Bootstrap, Repo, Tooling, Decisions

**Goal:** A clean working tree, public GitHub repo, .NET solution skeleton, Vite React skeleton, .gitignore correctness, README + prompts skeletons, no application code yet.

**Subagents/Skills:** `ecc:project-init` (sanity scan), `ecc:planner` (expand into tasks).

**Scope (logical units → one commit each):**
1. Initialize git in `D:\SindujaKannan_Docs\AA_Interview_Assignment\AA_Internview_Claude_Code\`, create `.gitignore` covering `bin/`, `obj/`, `node_modules/`, `.vs/`, `*.user`, `appsettings.Development.json`, `*.pfx`, `*.pdf` (so the brief PDF stays untracked), `dist/`, `coverage/`, `.idea/`.
2. Create empty `README.md` with a status table (one row per phase, status column initially all `Planned`) and required sections: How to run, Stack & rationale, Status, Known rough edges, AI tools used, Planning, Prompts archive link, Future work.
3. Create `docs/prompts.md` with the initial user prompt verbatim, plus the decisions table from this plan, timestamped.
4. Create `docs/planning.md` pointing to this plan file and to `docs/superpowers/plans/`.
5. Scaffold solution: `dotnet new sln -n AaSmr -o api`, `dotnet new webapi -n AaSmr.Api -o api/AaSmr.Api --use-controllers false` (minimal API), `dotnet new xunit -n AaSmr.Api.Tests -o api/AaSmr.Api.Tests`, `dotnet sln api/AaSmr.sln add api/AaSmr.Api api/AaSmr.Api.Tests`, project reference Tests → Api.
6. Replace Api's `appsettings.Development.json` with the real connection string (locally only); commit a redacted `appsettings.Development.example.json`. Verify `git status` does NOT list the real file.
7. Scaffold web: `npm create vite@latest web -- --template react-ts`, then `cd web && npm install`. Add Vitest, Testing Library, React Router, TanStack Query, Zod, Playwright as dev deps.
8. Add a top-level `package.json` (or document npm/dotnet commands in README) so a single command sequence runs everything.
9. Create GitHub repo: `gh repo create Sinduja-Kannan-93/aa-smr-scheduler --public --description "AA SMR Appointment Scheduler – interview build (React + .NET 8 + EF Core + MSSQL LocalDB)" --source . --remote origin`. Do NOT push yet — first push happens only after user approves.

**Approval gate:** Show `git log --oneline`, `git status`, repo URL from `gh repo view --json url`. Wait for "approved to push" before `git push -u origin main`.

**Phase 0 done when:** Local repo green, GitHub repo created, README + prompts skeleton committed, `appsettings.Development.json` confirmed gitignored, `dotnet build` + `npm run build` succeed on empty scaffolds.

---

## Phase 1 — Domain Model, EF Core DbContext, Migrations, Seed Data

**Goal:** Database schema applied automatically on first run, with 3 branches, 4 mechanics (each assigned to a branch), 4 service types, and appointment slots for the next 7 days; named seed users available for the Act-As dropdown.

**Subagents/Skills:** `ecc:planner`, `ecc:tdd-guide`, `ecc:csharp-reviewer`, `ecc:database-reviewer`, `superpowers:test-driven-development`.

**Entities & key constraints:**
- `Branch { Id, Name, City, Address }` — 3 rows: Dublin (Naas Rd), Cork (Kinsale Rd), Galway (Tuam Rd).
- `ServiceType { Id, Code, Name, DurationMinutes }` — 4 rows: Inspection (60), Service (90), Repair (120), Diagnostics (45).
- `Mechanic { Id, FullName, BranchId }` — 4 rows, each tied to a branch. Galway gets two so admin view has variety.
- `User { Id, FullName, Role (BookingAgent|Mechanic|Admin), MechanicId? }` — named seed users for Act-As. Example seeds: `Niamh O'Sullivan` (BookingAgent), `John Byrne` (Mechanic, Dublin), `Mary O'Brien` (Mechanic, Cork), `Liam Walsh` (Mechanic, Galway), `Aoife Kelly` (Mechanic, Galway), `Admin Manager` (Admin).
- `AppointmentSlot { Id, BranchId, MechanicId, ServiceTypeId, StartUtc, EndUtc, IsBooked }` — generated for next 7 days × working hours × per mechanic. Unique index `(MechanicId, StartUtc)`.
- `Appointment { Id, ReferenceNumber (unique), SlotId (unique, FK), CustomerName, CustomerPhone, VehicleRegistration, Notes, Status (Scheduled|InProgress|Completed|NoShow), CreatedUtc, BookingAgentUserId }`. Unique index on `SlotId` enforces no double-booking at the database level.
- `WorkNote { Id, AppointmentId, Body, AuthorMechanicId, CreatedUtc }`.

**Scope (logical units → one commit each):**
1. Add NuGet packages: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.InMemory` (tests), `FluentAssertions` (tests), `Microsoft.AspNetCore.Mvc.Testing` (tests).
2. Define `Domain/` entities one file at a time (Branch first, then ServiceType, Mechanic, User, AppointmentSlot, Appointment, WorkNote). Each entity gets a failing unit test first that asserts shape (e.g., `Appointment.ReferenceNumber must be unique`, via in-memory context).
3. Define `Data/AppDbContext.cs` and per-entity `IEntityTypeConfiguration<T>` classes. Test that unique indexes are configured (introspect `Model.FindEntityType(...).GetIndexes()`).
4. Add EF Tools: `dotnet tool install --global dotnet-ef` (verify version), generate the initial migration: `dotnet ef migrations add Initial -p api/AaSmr.Api`. Test that migration file is present and contains expected tables.
5. Implement `DbInitializer.SeedAsync(AppDbContext)` — idempotent (check counts before inserting). Generate slots for next 7 days at 09:00–17:00 in 1-hour increments per mechanic, alternating service types so filters are meaningful. Tests: re-running seed does not duplicate; count assertions per table.
6. Wire `Program.cs` to call `await db.Database.MigrateAsync(); await DbInitializer.SeedAsync(db);` inside an `app.Services.CreateScope()` before `app.Run()`. Integration test using `WebApplicationFactory` + a unique LocalDB DB name (or `Microsoft.EntityFrameworkCore.Sqlite` in-memory if LocalDB makes CI flaky) confirms tables exist after startup.
7. Add CORS for `http://localhost:5173` (Vite default) in Development only.

**Approval gate:** Show migration files, seed counts (`SELECT COUNT(*) ...` via a quick `dotnet run` script), and `dotnet test` output. Wait for approval. Then commit each logical unit individually.

**Phase 1 done when:** Running `dotnet run --project api/AaSmr.Api` from an empty database creates the schema and seed, twice in a row without duplicate rows. Tests green.

---

## Phase 2 — Backend API Endpoints

**Goal:** All endpoints needed for the three flows, with double-booking prevention and reference-number generation, integration-tested via `WebApplicationFactory`.

**Subagents/Skills:** `ecc:planner`, `ecc:tdd-guide`, `ecc:csharp-reviewer`, `ecc:security-reviewer`, `superpowers:test-driven-development`.

**Endpoints (Minimal API, grouped by feature folder):**
- `GET  /api/users` — list named seed users for Act-As, with role + linked mechanic.
- `GET  /api/branches`
- `GET  /api/service-types`
- `GET  /api/slots?from&to&serviceTypeId?&branchId?` — only `IsBooked = false`, next 7 days by default, ordered by `StartUtc`.
- `POST /api/appointments` body `{ slotId, customerName, customerPhone, vehicleRegistration, notes, bookingAgentUserId }` — atomic: open transaction, re-check `IsBooked`, flip to `true`, insert `Appointment` with generated `ReferenceNumber` like `SMR-2026-XXXXXX` (6-char base32). On conflict (unique-index violation or already-booked) return `409 Conflict` with a structured error. Validate inputs with `MiniValidation` or hand-rolled rules (Zod-equivalent server side).
- `GET  /api/appointments/today` — admin view: all today's appointments grouped by mechanic.
- `GET  /api/appointments?mechanicId&from&to` — mechanic's list for today + tomorrow.
- `GET  /api/appointments/{id}` — detail with customer, vehicle, slot, notes.
- `POST /api/appointments/{id}/notes` body `{ body, authorMechanicId }` — append timestamped `WorkNote`.
- `PATCH /api/appointments/{id}/status` body `{ status }` — enforce state machine: `Scheduled → InProgress → Completed` or `Scheduled → NoShow` or `InProgress → Completed`. Reject illegal transitions with `400`.

**Cross-cutting:**
- `ApiResponse<T> { success, data?, error?, meta? }` envelope.
- Exception middleware → consistent 400/404/409/500 JSON.
- Logging via `ILogger<T>`.
- Reference-number generation collision-safe (retry on unique violation).

**Scope (logical units → one commit each):**
1. Add `ApiResponse<T>` + exception middleware. Test middleware turns thrown `ValidationException` into 400 envelope.
2. Users + Branches + ServiceTypes endpoints (read-only).
3. Slots query with filters. Tests: filter by service type, by branch, excludes booked, window defaults to next 7 days.
4. Book appointment endpoint with transactional double-booking prevention. Tests: happy path returns 200 + reference; second booking of same slot returns 409; concurrent bookings — spin two `Task`s and assert exactly one wins.
5. Today's appointments + mechanic-scoped list. Tests: scoping correctness, ordering, date-window edge.
6. Appointment detail.
7. Work notes append.
8. Status state machine. Tests for each legal + illegal transition.
9. Security pass: `ecc:security-reviewer` run; address any input-validation, error-leakage, or SQL-injection (parameterization is automatic via EF, but spot-check raw queries) findings.

**Approval gate:** Show `dotnet test` output (all green), Postman/curl call log for each endpoint or a saved `.http` file under `api/AaSmr.Api/Api.http`. Wait. Commit per unit.

**Phase 2 done when:** All endpoints answer correctly via `.http` calls, double-booking concurrency test passes, security review reports no CRITICAL/HIGH.

---

## Phase 3 — Frontend Scaffold, Identity Context, Design System

**Goal:** Vite/React/TS app with React Router, TanStack Query, Zod-validated API client, an `IdentityContext` powering the Act-As dropdown of **named seed users**, a brand-aligned design system rooted in `PRODUCT.md` + `DESIGN.md`, and the cross-cutting **role-switch reset** behavior fully tested.

**Subagents/Skills:** `impeccable teach` (one-shot to generate PRODUCT.md and DESIGN.md from theaa.ie tone and the brief's domain), `impeccable shape` (plan IA + layout shell), `ui-ux-pro-max:ui-ux-pro-max` (component-level direction), `ecc:typescript-reviewer`, `ecc:tdd-guide`, `superpowers:test-driven-development`.

**Scope (logical units → one commit each):**
1. Run `impeccable teach` with explicit context: AA Ireland tone (https://www.theaa.ie/), internal SMR tool register = `product` (not brand), users = booking agents + mechanics + admins, key anti-references ("not a flashy consumer marketing page", "not a generic Material dashboard", "not template SaaS"). Capture outputs in `web/PRODUCT.md` and `web/DESIGN.md`.
2. Define `web/src/styles/tokens.css` from `DESIGN.md`: OKLCH palette anchored to AA yellow (`oklch(~85% 0.18 95)`) + near-black ink + tinted neutrals; type scale with ≥1.25 ratio; spacing scale; motion tokens (ease-out-quart). No `#000`, no `#fff`, no side-stripe borders, no gradient text, no glassmorphism.
3. Install + configure React Router 6, TanStack Query (with `QueryClient` exposed for reset), and a small typed API client in `web/src/lib/apiClient.ts` using `fetch` + Zod schemas mirroring backend DTOs.
4. Build `web/src/features/identity/`:
   - `IdentityContext` with `{ user: User | null, setUser, queryClient }`.
   - `useIdentity()` hook.
   - `ActAsDropdown` component reading `GET /api/users`, showing named seed users grouped by role.
   - **Reset behavior on change:** `queryClient.clear()` + navigate to the role's home route + a small `useEffect`-driven local-state reset signal that pages can subscribe to (or simply re-mount the route subtree by keying it on `user.id`).
5. Layout shell: `<AppShell>` with header (AA-tinted brand mark + product name "SMR Scheduler"), Act-As dropdown, role-aware primary nav.
6. UI primitives shaped via `impeccable craft`: `Button`, `Input`, `Select`, `Card`, `Badge` (status), `EmptyState`, `Dialog` (booking confirmation). Built with discipline against the absolute bans (no side-stripe borders, no nested cards).
7. **Tests (TDD):**
   - `IdentityContext` defaults to `null`; setUser updates context.
   - `ActAsDropdown` lists users from API mock, calls `setUser` on selection, calls `queryClient.clear()`, navigates to expected route per role.
   - Switching from Mechanic→Admin re-renders with no stale Mechanic-scoped query data visible (assert via mocked TanStack Query state).
   - Tokens snapshot test for design tokens.

**Approval gate:** Run `npm run dev`, show the shell + dropdown screenshot, `npm test` output, plus a short Loom-style description. Wait. Commit per unit.

**Phase 3 done when:** Switching roles in the dropdown navigates and resets the cache; no page leaks stale data across role switches; tests green; `npm run build` clean.

---

## Phase 4 — Booking Agent Flow (UI)

**Goal:** Booking agent can list available slots for the next 7 days, filter by service type and branch, book a slot with all required fields, and see a confirmation with the reference number. Double-booking surfaces a clear, recoverable error.

**Subagents/Skills:** `impeccable shape` (per surface), `impeccable craft`, `impeccable critique`, `ui-ux-pro-max:ui-ux-pro-max`, `ecc:tdd-guide`, `ecc:typescript-reviewer`, `superpowers:test-driven-development`.

**Surfaces:**
- `/booking` (default for Booking Agent role) — `SlotsList` with `SlotFilterBar` (service type, branch, date) and per-slot "Book" CTA.
- `/booking/new?slotId=...` — `BookingForm` (customer name, phone, vehicle reg, service type pre-filled from slot, branch from slot, notes). Zod validation client-side mirrored on server.
- `/booking/confirmation/:appointmentId` — `Confirmation` view with `SMR-2026-XXXXXX` reference and a "Book another" CTA.

**Scope (logical units → one commit each):**
1. `useSlots` hook (TanStack Query, filters in URL params). Tests: re-fetches on filter change, debounces, derives empty state.
2. `SlotFilterBar` with service-type chips + branch select + date picker (single-day or range). Tests for keyboard navigation, ARIA, and that filters survive a role switch only if user is still Booking Agent (otherwise cleared by Phase 3's reset).
3. `SlotsList` grid (NOT identical card grid — use a list with strong rhythm, status, time-block emphasis). `impeccable critique` pass.
4. `BookingForm` with Zod schema; phone validated as Irish-friendly pattern (`+353…` or local), vehicle reg sanitized (uppercase, no whitespace). Tests for each validation rule.
5. `useBookSlot` mutation; optimistic update of slot list; rollback on 409 with toast/inline error and re-fetch.
6. `Confirmation` page showing reference number prominently with a copy-to-clipboard action.
7. Accessibility audit via `impeccable audit`: keyboard reachable, focus order, contrast, reduced-motion respect.

**Approval gate:** Show screenshots for each surface + `npm test` green + a manual walkthrough video or step-list. Wait. Commit per unit.

**Phase 4 done when:** A booking agent in the seed list (`Niamh O'Sullivan`) can complete a booking end-to-end; the booked slot disappears from the list; the same slot booked twice in fast succession yields a clean 409 UI.

---

## Phase 5 — Mechanic Flow (UI)

**Goal:** A mechanic seed user can see appointments assigned to them for today + tomorrow, open one, read customer/vehicle/notes, add a timestamped work note, and advance status through the legal state machine.

**Subagents/Skills:** Same as Phase 4.

**Surfaces:**
- `/mechanic` (default for Mechanic role) — `AppointmentList` grouped by Today / Tomorrow.
- `/mechanic/appointments/:id` — `AppointmentDetail` with customer block, vehicle block, customer notes, `WorkNoteList`, "Add note" composer, and `StatusStepper` (Scheduled / In Progress / Completed / No-Show).

**Scope (logical units → one commit each):**
1. `useMyAppointments` hook scoped to `identity.user.mechanicId`; reacts to identity changes (re-keyed by mechanicId so a switch fully refetches).
2. `AppointmentList` with strong time-of-day hierarchy, status badge, customer name. Empty state for "No appointments today."
3. `AppointmentDetail` layout (asymmetric — not a card grid; varied spacing, intentional hierarchy per Impeccable laws).
4. `useAddWorkNote` mutation; optimistic append; rollback on failure.
5. `StatusStepper` enforcing the same state machine as the backend, with confirmation dialog for `No-Show` and `Completed`.
6. Tests:
   - Switching from John Byrne (Dublin) → Mary O'Brien (Cork) clears Dublin appointments before Cork's load resolves.
   - Status transitions: legal allowed, illegal disabled.
   - Work-note timestamp is server-set, not client-set.

**Approval gate:** Same shape as Phase 4.

**Phase 5 done when:** A mechanic seed user can manage at least one full lifecycle of an appointment created in Phase 4.

---

## Phase 6 — Admin Home (UI)

**Goal:** Admin seed user sees today's schedule across all mechanics, grouped by mechanic (or by branch — decide during `impeccable shape`).

**Subagents/Skills:** Same as Phase 4.

**Surfaces:**
- `/` (default for Admin role) — `TodayAcrossMechanics` panel. Possibly secondary "By branch" view toggle if `impeccable shape` recommends.

**Scope (logical units → one commit each):**
1. `useTodaySchedule` hook hitting `/api/appointments/today`.
2. `TodayAcrossMechanics` layout — editorial composition; status legend; counts per mechanic. Avoid the "hero metric template" anti-pattern.
3. Empty state for a quiet day. `impeccable critique` pass.
4. Tests: data grouping correctness, role-switch out of admin clears the cached today view.

**Approval gate:** Same shape.

**Phase 6 done when:** Admin sees the consolidated daily view; switching to Mechanic or Booking Agent role clears it and lands on the appropriate home.

---

## Phase 7 — Polish, E2E, Final Verification, README finalization

**Goal:** A first-time clone runs end-to-end with one short command sequence, all tests green, a Playwright smoke run exercises the three golden paths, the README tells a clear story, and `docs/prompts.md` is complete.

**Subagents/Skills:** `impeccable polish`, `impeccable audit`, `ecc:e2e-runner`, `ecc:doc-updater`, `ecc:code-reviewer`, `superpowers:verification-before-completion`, `superpowers:finishing-a-development-branch`.

**Scope (logical units → one commit each):**
1. `impeccable polish` pass across all surfaces; commit per surface adjustment.
2. `impeccable audit` (a11y + responsive + performance hints). Address findings.
3. Playwright E2E smoke under `web/e2e/`:
   - Booking agent books a slot and lands on confirmation with a reference number.
   - Mechanic opens that appointment, adds a note, advances to In Progress, then Completed.
   - Admin sees the appointment in today's view.
4. Optional `docker-compose.yml` + Dockerfiles for `api` and `web` (only if time remains; otherwise listed as future work).
5. Finalize README: stack & rationale (React + .NET 8 chosen over Blazor for clearer separation of concerns and better front-end iteration speed with Vite), "How to run" with the exact two-command sequence (`dotnet run --project api/AaSmr.Api` and `npm --prefix web run dev`) plus the LocalDB prerequisite, status table all `Done`, what's done / not done, known rough edges, **AI tools used** (Claude Code Opus 4.7 + listed subagents/skills), **Planning** (link to this plan + per-phase plans), **Prompts archive** link, **Future work** (the brief's out-of-scope list + anything deferred during execution).
6. Final code review with `ecc:code-reviewer` on the full diff vs. initial commit; address CRITICAL/HIGH only.
7. `superpowers:verification-before-completion` checklist run and recorded in `docs/prompts.md`.
8. Stage final commits, wait for approval, then `git push`.

**Approval gate:** Show the final README rendered, Playwright artifacts (screenshots/videos), test counts, repo URL. Wait. Push.

**Phase 7 done when:** A fresh `git clone` + the documented steps produce a working app on a clean machine in under five minutes.

---

## Critical Files Index

| Path | Responsibility |
|---|---|
| `api/AaSmr.Api/Program.cs` | Compose DI, configure EF, run `MigrateAsync` + `DbInitializer.SeedAsync` on startup, register endpoints, enable CORS, exception middleware. |
| `api/AaSmr.Api/Data/AppDbContext.cs` | EF context + `DbSet`s + `OnModelCreating` calling per-entity configurations. |
| `api/AaSmr.Api/Data/DbInitializer.cs` | Idempotent seed: 3 branches, 4 mechanics, 4 service types, 7 days × hourly slots, named users incl. `Niamh O'Sullivan` (BookingAgent), 4 named mechanics, `Admin Manager`. |
| `api/AaSmr.Api/Features/Appointments/BookAppointmentHandler.cs` | Transactional book; reference-number generation; 409 on conflict. |
| `api/AaSmr.Api/Features/Appointments/StatusTransition.cs` | State machine validation. |
| `api/AaSmr.Api/appsettings.Development.example.json` | Committed template with placeholder connection string. |
| `api/AaSmr.Api/appsettings.Development.json` | **Gitignored.** Real LocalDB connection string. |
| `web/src/features/identity/IdentityContext.tsx` | Identity context + `queryClient.clear()` on user change + route navigation per role. |
| `web/src/features/identity/ActAsDropdown.tsx` | Named-seed-user dropdown bound to `GET /api/users`. |
| `web/src/App.tsx` | Routes keyed by `identity.user?.id` for full subtree reset on role switch. |
| `web/PRODUCT.md`, `web/DESIGN.md` | Impeccable context for register, palette, type, motion. |
| `web/src/styles/tokens.css` | OKLCH tokens, type scale, motion curves. |
| `.gitignore` | `appsettings.Development.json`, `*.pdf`, `bin/`, `obj/`, `node_modules/`, `dist/`, `.vs/`, `*.user`. |
| `README.md` | Stack, run, status table, AI tools, planning, prompts, future work. |
| `docs/prompts.md` | Prompt archive, appended per phase. |

---

## End-to-End Verification (Phase 7 exit criteria)

1. Fresh terminal: `git clone https://github.com/Sinduja-Kannan-93/aa-smr-scheduler.git`.
2. `cp api/AaSmr.Api/appsettings.Development.example.json api/AaSmr.Api/appsettings.Development.json` and adjust the connection string if needed (default LocalDB works on the user's machine).
3. `dotnet run --project api/AaSmr.Api` — migration runs, seed runs, API listens.
4. `npm --prefix web install && npm --prefix web run dev` — Vite dev server on `http://localhost:5173`.
5. Open `http://localhost:5173`. Defaults to whatever role the first seed user has; switch Act-As to `Niamh O'Sullivan`. Book a slot. Note the reference number.
6. Switch Act-As to `John Byrne`. The list resets, no stale agent data. Open the new appointment, add a note, transition to In Progress, then Completed.
7. Switch Act-As to `Admin Manager`. The view resets to today's consolidated schedule and shows the appointment.
8. `dotnet test` and `npm --prefix web test` both green. `npx playwright test` smoke green.
9. `git log --oneline` shows many descriptive commits, not one giant commit.
10. README accurate; out-of-scope items present under "Future work"; prompts archive linked.

---

## Future Work (pre-seeded into README on Phase 7)

Documented but not implemented:
- Authentication / login (currently Act-As named-user dropdown).
- Email or SMS notifications on booking confirmation and status changes.
- Rescheduling and cancellation flows.
- Recurring appointments (e.g., fleet vehicles every 6 months).
- Payments / invoicing.
- Mobile-specific UI (current build is responsive but not mobile-app-shaped).
- Production-grade config: secrets in environment / Key Vault rather than `appsettings.Development.json`; migrations gated behind a deploy step, not auto-applied at startup.
- Multi-tenant separation (multiple AA branches as tenants).
- Audit log of status changes and note edits.
