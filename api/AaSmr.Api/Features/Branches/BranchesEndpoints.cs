using AaSmr.Api.Data;
using AaSmr.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Features.Branches;

public static class BranchesEndpoints
{
    public static void MapBranches(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/branches", async (AppDbContext db, CancellationToken ct) =>
        {
            var branches = await db.Branches
                .OrderBy(b => b.Name)
                .Select(b => new BranchDto(b.Id, b.Name, b.City, b.Address))
                .ToListAsync(ct);

            return Results.Ok(new ApiResponse<List<BranchDto>>(true, branches));
        })
        .WithName("GetBranches")
        .WithOpenApi();
    }
}
