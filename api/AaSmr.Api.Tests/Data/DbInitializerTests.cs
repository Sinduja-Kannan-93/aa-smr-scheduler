using AaSmr.Api.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Tests.Data;

public class DbInitializerTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task SeedAsync_PopulatesThreeBranches()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        var branches = await db.Branches.ToListAsync();
        branches.Should().HaveCount(3);
        branches.Select(b => b.Name).Should().Contain(["Dublin", "Cork", "Galway"]);
    }

    [Fact]
    public async Task SeedAsync_PopulatesFourServiceTypes()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        var types = await db.ServiceTypes.ToListAsync();
        types.Should().HaveCount(4);
        types.Select(t => t.Code).Should().Contain(["Inspection", "Service", "Repair", "Diagnostics"]);
        types.Single(t => t.Code == "Inspection").DurationMinutes.Should().Be(60);
        types.Single(t => t.Code == "Service").DurationMinutes.Should().Be(90);
        types.Single(t => t.Code == "Repair").DurationMinutes.Should().Be(120);
        types.Single(t => t.Code == "Diagnostics").DurationMinutes.Should().Be(45);
    }

    [Fact]
    public async Task SeedAsync_PopulatesFourMechanicsAssignedToCorrectBranches()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        var mechanics = await db.Mechanics.ToListAsync();
        mechanics.Should().HaveCount(4);

        var branches = await db.Branches.ToListAsync();
        var dublinId = branches.Single(b => b.Name == "Dublin").Id;
        var corkId = branches.Single(b => b.Name == "Cork").Id;
        var galwayId = branches.Single(b => b.Name == "Galway").Id;

        mechanics.Single(m => m.FullName == "John Byrne").BranchId.Should().Be(dublinId);
        mechanics.Single(m => m.FullName == "Mary O'Brien").BranchId.Should().Be(corkId);
        mechanics.Single(m => m.FullName == "Liam Walsh").BranchId.Should().Be(galwayId);
        mechanics.Single(m => m.FullName == "Aoife Kelly").BranchId.Should().Be(galwayId);
    }

    [Fact]
    public async Task SeedAsync_PopulatesSixUsersWithCorrectRoles()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        var users = await db.Users.ToListAsync();
        users.Should().HaveCount(6);
        users.Count(u => u.Role == AaSmr.Api.Domain.UserRole.BookingAgent).Should().Be(1);
        users.Count(u => u.Role == AaSmr.Api.Domain.UserRole.Mechanic).Should().Be(4);
        users.Count(u => u.Role == AaSmr.Api.Domain.UserRole.Admin).Should().Be(1);
        users.Where(u => u.Role == AaSmr.Api.Domain.UserRole.Mechanic).Should().AllSatisfy(u => u.MechanicId.Should().NotBeNull());
        users.Single(u => u.Role == AaSmr.Api.Domain.UserRole.Admin).MechanicId.Should().BeNull();
    }

    [Fact]
    public async Task SeedAsync_PopulatesAppointmentSlots_TotalIs224()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        var slots = await db.AppointmentSlots.ToListAsync();
        slots.Should().HaveCount(224);
        slots.Should().AllSatisfy(s => s.IsBooked.Should().BeFalse());
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_RunningTwiceDoesNotDoubleRows()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        await DbInitializer.SeedAsync(db);
        (await db.Branches.CountAsync()).Should().Be(3);
        (await db.AppointmentSlots.CountAsync()).Should().Be(224);
    }

    [Fact]
    public async Task SeedAsync_SlotsSpanNextSevenDaysFromToday()
    {
        using var db = CreateContext();
        var today = DateTime.UtcNow.Date;
        await DbInitializer.SeedAsync(db, today);
        var slots = await db.AppointmentSlots.ToListAsync();
        slots.Should().AllSatisfy(s =>
        {
            s.StartUtc.Date.Should().BeOnOrAfter(today);
            s.StartUtc.Date.Should().BeBefore(today.AddDays(7));
        });
    }
}
