using AaSmr.Api.Data;
using AaSmr.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Features.ServiceTypes;

public static class ServiceTypesEndpoints
{
    public static void MapServiceTypes(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/service-types", async (AppDbContext db, CancellationToken ct) =>
        {
            var types = await db.ServiceTypes
                .OrderBy(t => t.Name)
                .Select(t => new ServiceTypeDto(t.Id, t.Code, t.Name, t.DurationMinutes))
                .ToListAsync(ct);

            return Results.Ok(new ApiResponse<List<ServiceTypeDto>>(true, types));
        })
        .WithName("GetServiceTypes")
        .WithOpenApi();
    }
}
