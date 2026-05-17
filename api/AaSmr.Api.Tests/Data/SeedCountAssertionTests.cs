using AaSmr.Api.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Tests.Data;

public class SeedCountAssertionTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact] public async Task AfterSeed_BranchCountIs3() { using var db = CreateContext(); await DbInitializer.SeedAsync(db); (await db.Branches.CountAsync()).Should().Be(3); }
    [Fact] public async Task AfterSeed_ServiceTypeCountIs4() { using var db = CreateContext(); await DbInitializer.SeedAsync(db); (await db.ServiceTypes.CountAsync()).Should().Be(4); }
    [Fact] public async Task AfterSeed_MechanicCountIs4() { using var db = CreateContext(); await DbInitializer.SeedAsync(db); (await db.Mechanics.CountAsync()).Should().Be(4); }
    [Fact] public async Task AfterSeed_UserCountIs6() { using var db = CreateContext(); await DbInitializer.SeedAsync(db); (await db.Users.CountAsync()).Should().Be(6); }
    [Fact] public async Task AfterSeed_AppointmentSlotCountIs224() { using var db = CreateContext(); await DbInitializer.SeedAsync(db); (await db.AppointmentSlots.CountAsync()).Should().Be(224); }
    [Fact]
    public async Task AfterDoubleSeed_AllCountsUnchanged()
    {
        using var db = CreateContext();
        await DbInitializer.SeedAsync(db);
        await DbInitializer.SeedAsync(db);
        (await db.Branches.CountAsync()).Should().Be(3);
        (await db.ServiceTypes.CountAsync()).Should().Be(4);
        (await db.Mechanics.CountAsync()).Should().Be(4);
        (await db.Users.CountAsync()).Should().Be(6);
        (await db.AppointmentSlots.CountAsync()).Should().Be(224);
    }
}
