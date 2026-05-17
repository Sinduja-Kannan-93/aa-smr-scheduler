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
