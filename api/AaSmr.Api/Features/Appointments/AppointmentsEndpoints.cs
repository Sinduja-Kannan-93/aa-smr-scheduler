using AaSmr.Api.Data;
using AaSmr.Api.Domain;
using AaSmr.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Features.Appointments;

public static class AppointmentsEndpoints
{
    public static void MapAppointments(this IEndpointRouteBuilder app)
    {
        // Must be registered before /{id:guid} so literal "today" is matched first
        app.MapGet("/api/appointments/today", async (AppDbContext db, CancellationToken ct) =>
        {
            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            var appointments = await db.Appointments
                .Include(a => a.Slot).ThenInclude(s => s.ServiceType)
                .Include(a => a.Slot).ThenInclude(s => s.Mechanic).ThenInclude(m => m.Branch)
                .Where(a => a.Slot.StartUtc >= todayStart && a.Slot.StartUtc < todayEnd)
                .OrderBy(a => a.Slot.Mechanic.FullName)
                .ThenBy(a => a.Slot.StartUtc)
                .ToListAsync(ct);

            var grouped = appointments
                .GroupBy(a => a.Slot.MechanicId)
                .Select(g => new MechanicScheduleDto(
                    g.Key,
                    g.First().Slot.Mechanic.FullName,
                    g.First().Slot.Mechanic.Branch.Name,
                    g.Select(ToListItem).ToList()))
                .ToList();

            return Results.Ok(new ApiResponse<List<MechanicScheduleDto>>(true, grouped));
        })
        .WithName("GetTodayAppointments")
        .WithOpenApi();

        app.MapGet("/api/appointments", async (
            Guid? mechanicId,
            DateTime? from,
            DateTime? to,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var start = (from ?? DateTime.UtcNow).Date;
            var end = to.HasValue ? to.Value.Date.AddDays(1) : start.AddDays(2);

            var query = db.Appointments
                .Include(a => a.Slot).ThenInclude(s => s.ServiceType)
                .Include(a => a.Slot).ThenInclude(s => s.Mechanic).ThenInclude(m => m.Branch)
                .Where(a => a.Slot.StartUtc >= start && a.Slot.StartUtc < end);

            if (mechanicId.HasValue)
                query = query.Where(a => a.Slot.MechanicId == mechanicId.Value);

            var appointments = await query
                .OrderBy(a => a.Slot.StartUtc)
                .ToListAsync(ct);

            return Results.Ok(new ApiResponse<List<AppointmentListItemDto>>(true,
                appointments.Select(ToListItem).ToList()));
        })
        .WithName("GetAppointments")
        .WithOpenApi();

        app.MapGet("/api/appointments/{id:guid}", async (
            Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var a = await db.Appointments
                .Include(a => a.Slot).ThenInclude(s => s.ServiceType)
                .Include(a => a.Slot).ThenInclude(s => s.Mechanic).ThenInclude(m => m.Branch)
                .Include(a => a.WorkNotes).ThenInclude(n => n.AuthorMechanic)
                .FirstOrDefaultAsync(a => a.Id == id, ct);

            if (a is null)
                throw new NotFoundException($"Appointment {id} not found.");

            var dto = new AppointmentDetailDto(
                a.Id,
                a.ReferenceNumber,
                a.CustomerName,
                a.CustomerPhone,
                a.VehicleRegistration,
                a.Notes,
                a.Status.ToString(),
                a.CreatedUtc,
                a.Slot.StartUtc,
                a.Slot.EndUtc,
                a.Slot.ServiceType.Name,
                a.Slot.Mechanic.FullName,
                a.Slot.Mechanic.Branch.Name,
                a.WorkNotes
                    .OrderBy(n => n.CreatedUtc)
                    .Select(n => new WorkNoteDto(n.Id, n.Body, n.AuthorMechanic.FullName, n.CreatedUtc))
                    .ToList());

            return Results.Ok(new ApiResponse<AppointmentDetailDto>(true, dto));
        })
        .WithName("GetAppointmentById")
        .WithOpenApi();

        app.MapPost("/api/appointments", async (
            BookAppointmentRequest request,
            AppDbContext db,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            Validate(request);

            var slot = await db.AppointmentSlots.FindAsync([request.SlotId], ct);
            if (slot is null)
                throw new NotFoundException($"Slot {request.SlotId} not found.");
            if (slot.IsBooked)
                throw new ConflictException("This slot has already been booked.");

            slot.IsBooked = true;

            var refNumber = await GenerateUniqueRefAsync(db, ct);

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                SlotId = request.SlotId,
                ReferenceNumber = refNumber,
                CustomerName = request.CustomerName.Trim(),
                CustomerPhone = request.CustomerPhone.Trim(),
                VehicleRegistration = request.VehicleRegistration.Trim().ToUpperInvariant(),
                Notes = request.Notes?.Trim(),
                Status = AppointmentStatus.Scheduled,
                CreatedUtc = DateTime.UtcNow,
                BookingAgentUserId = request.BookingAgentUserId
            };

            db.Appointments.Add(appointment);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                logger.LogWarning("Double-booking attempt on slot {SlotId}", request.SlotId);
                throw new ConflictException("This slot has already been booked.");
            }

            return Results.Ok(new ApiResponse<BookAppointmentResponse>(true,
                new BookAppointmentResponse(appointment.Id, refNumber)));
        })
        .WithName("BookAppointment")
        .WithOpenApi();

        app.MapPost("/api/appointments/{id:guid}/notes", async (
            Guid id,
            AddWorkNoteRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Body))
                throw new ValidationException("Body is required.");

            var appointment = await db.Appointments.FindAsync([id], ct);
            if (appointment is null)
                throw new NotFoundException($"Appointment {id} not found.");

            var mechanic = await db.Mechanics.FindAsync([request.AuthorMechanicId], ct);
            if (mechanic is null)
                throw new NotFoundException($"Mechanic {request.AuthorMechanicId} not found.");

            var note = new WorkNote
            {
                Id = Guid.NewGuid(),
                AppointmentId = id,
                Body = request.Body.Trim(),
                AuthorMechanicId = request.AuthorMechanicId,
                CreatedUtc = DateTime.UtcNow
            };

            db.WorkNotes.Add(note);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new ApiResponse<WorkNoteDto>(true,
                new WorkNoteDto(note.Id, note.Body, mechanic.FullName, note.CreatedUtc)));
        })
        .WithName("AddWorkNote")
        .WithOpenApi();

        app.MapPatch("/api/appointments/{id:guid}/status", async (
            Guid id,
            PatchStatusRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<AppointmentStatus>(request.Status, ignoreCase: true, out var newStatus))
                throw new ValidationException(
                    $"Invalid status '{request.Status}'. Valid: Scheduled, InProgress, Completed, NoShow.");

            var appointment = await db.Appointments.FindAsync([id], ct);
            if (appointment is null)
                throw new NotFoundException($"Appointment {id} not found.");

            if (!StatusTransition.IsValid(appointment.Status, newStatus))
                throw new ValidationException(
                    $"Cannot transition from {appointment.Status} to {newStatus}.");

            appointment.Status = newStatus;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new ApiResponse<object>(true, new { id, status = newStatus.ToString() }));
        })
        .WithName("PatchAppointmentStatus")
        .WithOpenApi();
    }

    private static void Validate(BookAppointmentRequest request)
    {
        var errors = new List<string>();
        if (request.SlotId == Guid.Empty) errors.Add("SlotId is required.");
        if (string.IsNullOrWhiteSpace(request.CustomerName)) errors.Add("CustomerName is required.");
        if (string.IsNullOrWhiteSpace(request.CustomerPhone)) errors.Add("CustomerPhone is required.");
        if (string.IsNullOrWhiteSpace(request.VehicleRegistration)) errors.Add("VehicleRegistration is required.");
        if (errors.Count > 0) throw new ValidationException(string.Join(" ", errors));
    }

    private static async Task<string> GenerateUniqueRefAsync(AppDbContext db, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = ReferenceNumberGenerator.Generate(year);
            if (!await db.Appointments.AnyAsync(a => a.ReferenceNumber == candidate, ct))
                return candidate;
        }
        throw new InvalidOperationException("Failed to generate a unique reference number after 10 attempts.");
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("unique constraint", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase);
    }

    private static AppointmentListItemDto ToListItem(Appointment a) =>
        new(a.Id, a.ReferenceNumber, a.CustomerName, a.CustomerPhone, a.VehicleRegistration,
            a.Status.ToString(), a.Slot.StartUtc, a.Slot.EndUtc,
            a.Slot.ServiceType.Name, a.Slot.Mechanic.FullName, a.Slot.Mechanic.Branch.Name);
}
