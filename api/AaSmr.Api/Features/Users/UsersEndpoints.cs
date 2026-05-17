using AaSmr.Api.Data;
using AaSmr.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Features.Users;

public static class UsersEndpoints
{
    public static void MapUsers(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (AppDbContext db, CancellationToken ct) =>
        {
            var users = await db.Users
                .Include(u => u.Mechanic)
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync(ct);

            var dtos = users.Select(u => new UserDto(
                u.Id,
                u.FullName,
                u.Role.ToString(),
                u.MechanicId,
                u.Mechanic?.FullName,
                u.Mechanic?.BranchId))
                .ToList();

            return Results.Ok(new ApiResponse<List<UserDto>>(true, dtos));
        })
        .WithName("GetUsers")
        .WithOpenApi();
    }
}
