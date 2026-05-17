using AaSmr.Api.Data;
using AaSmr.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Features.Slots;

public static class SlotsEndpoints
{
    public static void MapSlots(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/slots", async (
            DateTime? from,
            DateTime? to,
            Guid? serviceTypeId,
            Guid? branchId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var start = (from ?? DateTime.UtcNow).Date;
            var end = to.HasValue ? to.Value.Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(7);

            var query = db.AppointmentSlots
                .Where(s => !s.IsBooked && s.StartUtc >= start && s.StartUtc < end);

            if (serviceTypeId.HasValue)
                query = query.Where(s => s.ServiceTypeId == serviceTypeId.Value);
            if (branchId.HasValue)
                query = query.Where(s => s.BranchId == branchId.Value);

            var slots = await query
                .OrderBy(s => s.StartUtc)
                .Select(s => new SlotDto(
                    s.Id,
                    s.BranchId,
                    s.Branch.Name,
                    s.MechanicId,
                    s.Mechanic.FullName,
                    s.ServiceTypeId,
                    s.ServiceType.Name,
                    s.ServiceType.DurationMinutes,
                    s.StartUtc,
                    s.EndUtc))
                .ToListAsync(ct);

            return Results.Ok(new ApiResponse<List<SlotDto>>(true, slots));
        })
        .WithName("GetSlots")
        .WithOpenApi();
    }
}
