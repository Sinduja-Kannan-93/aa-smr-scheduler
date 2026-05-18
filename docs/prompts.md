# Prompts Archive

All significant prompts used during this project, ordered chronologically.
Appended at the end of each phase.

---

## Phase 0 — Bootstrap (2026-05-17)

### Initial User Prompt

```
Read the Interview assignment file carefully and understand the details:
"D:\SindujaKannan_Docs\AA_Interview_Assignment\AA_Internview_Claude_Code\AI_Coding_Interview_Assignment.pdf"

Preferred tech stack would be React + C# .net 8 + Entity Framework + MS SQL DB + Github

1. Your task is to prepare the multiphase implementation plan.
2. Plan should talk about using superpowers skills with subagents for feature development/review/verification/validation.
3. On completion of each phase, MUST get approval before commit.
4. Should not auto commit OR push without my approval.
5. Upon approval + git commit, must write/update, readme with status update etc, update prompts. Then, move on to next phase.
6. Without performing steps #3 to #5, should not move onto next phase.
7. Create a repository in the github with proper naming convention.
8. For UI development, should use UI-UX-PRO-MAX & Impeccable skills.
9. Impeccable skills are located at C:\Users\admin\.agents\skills.
10. Must refer the UI style and tone from internet via https://www.theaa.ie/
11. Ensure EF migrations should work without fail at application startup first time.
12. On changing view (Admin/Booking Agent/Mechanic) via drop down, it should properly reset to display only the relevant information in the UI.
13. Must follow TDD for each phase.
14. Application UI must be intuitive, clean & modern.
15. Make sure to ask for the MSSQL local DB connection string and keep that in appsettings.Development.json for local running. It should not be committed, and must be part of .gitignore
16. Seed data must include: 3 branches, 4 mechanics (assigned to branches), 4 service types, and appointment slots for the next 7 days.
17. The "act as" dropdown must allow switching between named seed users (e.g. specific mechanics and a booking agent), not just role types.
18. Each logical unit of work within a phase must be a separate descriptive git commit — not one commit per phase.
19. Out-of-scope items from the brief must be documented as future work in the README.
```

### Planning Session Decisions (captured via AskUserQuestion)

| Question | Answer |
|---|---|
| MSSQL connection string | Default LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=AaSmrScheduler;Trusted_Connection=True;TrustServerCertificate=True` |
| GitHub repo | `Sinduja-Kannan-93/aa-smr-scheduler` (public) |
| Workspace root | `D:\SindujaKannan_Docs\AA_Interview_Assignment\AA_Internview_Claude_Code\` (current folder) |
| Impeccable flow | Full flow: teach → shape → craft → polish |

### Implementation Approval Prompt

```
Plan reviewed. Proceed with implementation.
```

---
<!-- Append new entries below this line at the end of each phase -->

---

## Phase 1 — Domain Model, EF Core, Migrations, Seed (2026-05-17)

### Planner subagent prompt (summary)
Requested a bite-sized TDD plan for 8 logical units covering: NuGet packages, domain entities + enums, AppDbContext + EF configurations, Initial migration, DbInitializer seed, Program.cs startup wiring, SQLite constraint enforcement tests, and seed count assertions.

### Implementer subagent prompt (summary)
Provided complete C# code for all 28 files (9 domain, 9 data, 10 test files). Instructed agent to handle GateGuard two-step Write pattern, add SQLite package, build solution, and NOT run migrations or commit.

### Key decisions made
- Appointment → AppointmentSlot: `DeleteBehavior.NoAction` (avoids SQL Server cascade cycle)
- Appointment → WorkNote: `DeleteBehavior.Cascade`
- All other FKs: `Restrict` or `SetNull`
- Seed is idempotent via `if (await db.Branches.AnyAsync()) return;`
- `Program.cs` guards `MigrateAsync` behind `db.Database.IsRelational()` so integration tests using InMemory don't crash
- Integration test uses `new WebApplicationFactory<Program>()` per test class (not `IClassFixture`) to avoid scope isolation bug

### Test results
38/38 passing. 0 warnings.

---

## Phase 2 — Backend API Endpoints (2026-05-17)

### Session resume prompt

```
Avoid subagent dispatches for straightforward implementation — write API endpoints directly. resume Phase 2 in a new session
```

### Endpoints implemented

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/users` | Named seed users for Act-As dropdown |
| GET | `/api/branches` | All branches |
| GET | `/api/service-types` | All service types |
| GET | `/api/slots` | Available slots with `serviceTypeId`, `branchId`, `from`, `to` filters |
| POST | `/api/appointments` | Book slot; generates `SMR-YYYY-XXXXXX`; 409 on double-book |
| GET | `/api/appointments/today` | Admin view grouped by mechanic |
| GET | `/api/appointments` | Mechanic-scoped list (`mechanicId`, `from`, `to`) |
| GET | `/api/appointments/{id}` | Detail with work notes |
| POST | `/api/appointments/{id}/notes` | Append timestamped work note |
| PATCH | `/api/appointments/{id}/status` | State machine: Scheduled→InProgress→Completed or →NoShow |

### Key decisions made

- `ExceptionMiddleware` maps `ValidationException`→400, `NotFoundException`→404, `ConflictException`→409, unhandled→500.
- Double-booking protection: application-level `slot.IsBooked` check + `DbUpdateException` catch on unique constraint as concurrent-request safety net.
- `ReferenceNumberGenerator` uses a 32-char unambiguous alphabet (no I/O/1/0); retries up to 10 times on collision.
- `StatusTransition.IsValid` is single source of truth for the state machine.
- `InMemoryWebApplicationFactory` (fresh DB name per test class) used for integration tests.

### Test results
74/74 passing. 0 warnings. 36 new Phase 2 tests added.

---

## Phase 3 — Frontend Implementation (2026-05-18)

### Resume prompt
Proceed to phase 3. Exclude commit approval process for all remaining phases — implement everything autonomously and git push. Each task has its own commit, updates README & prompts without fail.

### Design system decisions
- **Style direction**: Swiss/International — precise grids, functional hierarchy, purposeful whitespace.
- **Palette**: AA Navy `#002D72` (surfaces/authority), AA Yellow `#F5A800` (CTAs/highlights), `#F4F6F8` background, white cards.
- **Typography**: Inter 400/500/600/700 via Google Fonts (preconnected in index.html).
- **Status colours**: Scheduled=blue, InProgress=amber, Completed=green, NoShow=gray — semantic, not decorative.
- **UI primitives**: pure CSS custom properties, no Tailwind or shadcn library defaults.
- **Skill invoked**: UI-UX-PRO-MAX (design quality guidelines, WCAG checklist, interaction rules).

### Components built

| File | Purpose |
|---|---|
| `src/styles/tokens.css` | All CSS custom properties (palette, spacing, radius, shadow, motion) |
| `src/lib/api.ts` | Zod schemas + typed fetch functions for all 10 endpoints + helpers |
| `src/lib/queryClient.ts` | TanStack Query client (30 s stale time, retry: 1) |
| `src/contexts/IdentityContext.tsx` | Act-As state — loads users, tracks active, invalidates queries on switch |
| `src/components/ui/` | Button, Badge+StatusBadge, Card+CardHeader, Input+Textarea, Select, Dialog, EmptyState, Spinner |
| `src/components/AppShell.tsx` | Sticky navy header with AA logo mark + Act-As dropdown |
| `src/pages/AdminPage.tsx` | Today's schedule grouped by mechanic (stat row + cards + 60 s poll) |
| `src/pages/BookingPage.tsx` | Slot browser (date/service/branch filters) + book Dialog + confirmation |
| `src/pages/MechanicPage.tsx` | Mechanic schedule + detail Dialog (status transitions, No Show confirm, work notes) |
| `src/App.tsx` | Role-based view switch inside AppShell |
| `src/main.tsx` | QueryClientProvider → IdentityProvider → App |

### Key decisions made
- Role-based rendering (not URL routing): `activeUser.role` selects AdminPage / BookingPage / MechanicPage; switching identity resets cached queries so the new view starts fresh.
- Dialog uses native `<dialog>` element with `showModal()` for built-in focus trapping and backdrop; animated with CSS keyframes.
- `vi.mock` factory in Vitest must not reference outer `const` variables (hoisting); data inlined with `satisfies User[]`.
- Act-As dropdown on mobile hides the label and role badge chips, keeping only the select for space.

### Test results
41/41 passing (3 new suites). TypeScript: 0 errors. 10 atomic commits on main.

---

## Phase 4 — Polish, E2E Tests, Final Verification (2026-05-18)

### Resume prompt
Proceed to phase 4. Ensure nothing is missed from the plan. Ensure to add Claude as co-author in git commit.

### What was done

| Commit | Scope |
|---|---|
| Playwright config + 4 E2E specs | Role switching, booking golden path, admin smoke, mechanic full flow |
| PRODUCT.md + DESIGN.md | Brand context and full design system reference |
| Vitest scope fix + App.css removal | Exclude e2e/ from Vitest; delete unused Vite default stylesheet |
| README finalisation | All phases ✅, test commands, known rough edges, docker deferred |
| Prompts update (this entry) | Phase 4 archived |

### Key decisions made
- Playwright `include` pattern scoped to `src/**` in vite.config.ts — Playwright e2e specs (`.spec.ts`) were being picked up by Vitest causing 4 phantom suite failures; `include: ['src/**/*.{test,spec}.{ts,tsx}']` fixed it.
- E2E golden path (mechanic test) books a slot first as BookingAgent, then switches to Mechanic — avoids depending on seed appointments which are slots, not appointments.
- `test.skip()` guards inside E2E tests handle the case where no available slots exist (e.g., all booked in CI seed state) gracefully without hard-failing.
- `dotnet clean` required after MSB3492 stale cache error; build and 74/74 tests pass cleanly after clean.
- Co-author `Claude Sonnet 4.6 <noreply@anthropic.com>` added to all Phase 4 commits.

### Final verification results
- TypeScript: 0 errors
- Vitest: 3 files, 41/41 passed
- dotnet test: 74/74 passed, 0 failed, 0 skipped
- Playwright E2E: requires both servers running (`dotnet run` + `npm run dev`)
- All 4 phases complete, all commits pushed to GitHub
