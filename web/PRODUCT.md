# AA SMR Scheduler — Product Context

## Product Summary

An internal scheduling tool for the AA Service, Maintenance & Repair team. Replaces ad-hoc spreadsheets and shared calendars with a structured booking workflow that prevents double-bookings, keeps work notes with appointments, and gives every role a purpose-built view.

## Brand

- **Organisation:** Automobile Association Ireland (AA Ireland)
- **Brand site reference:** theaa.ie
- **Voice:** Professional, authoritative, helpful — the AA is a trusted roadside companion; the tool should feel like the organisation: dependable, clear, no-nonsense.
- **Primary colours:** Navy `#002D72` (authority, trust) + Yellow `#F5A800` (action, energy)
- **Not:** casual, playful, startup-aesthetic, or generic SaaS grey

## User Roles

| Role | Named Seed User | What They Do |
|---|---|---|
| **Admin** | Alice Admin | Views today's full schedule across all mechanics; monitors status at a glance |
| **Booking Agent** | Niamh O'Sullivan | Finds available slots by date/branch/service type; books for walk-in customers |
| **Mechanic** | Carlos / Dave / Siobhan / Pat | Views own schedule; updates appointment status; adds work notes |

## Core Flows

1. **Booking Agent books a slot** → customer details + vehicle reg → reference number (`SMR-YYYY-XXXXXX`)
2. **Mechanic works an appointment** → Scheduled → In Progress → Completed (or No Show)
3. **Mechanic records work** → timestamped work notes attached to the appointment
4. **Admin monitors** → today's view grouped by mechanic with live status badges

## Constraints

- Internal tool: no public-facing login; identity simulated via Act-As dropdown for demo/dev
- Runs on LocalDB in development; SQL Server in production
- Not: customer-facing, mobile-first, PWA, multi-tenant
