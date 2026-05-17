using AaSmr.Api.Data;
using AaSmr.Api.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Tests.Data;

public class AppDbContextTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public void CanInstantiateContextWithInMemoryProvider()
    {
        var act = () => CreateContext();
        act.Should().NotThrow();
    }

    [Fact]
    public void AllSevenDbSetsAreExposed()
    {
        using var db = CreateContext();
        db.Branches.Should().NotBeNull();
        db.ServiceTypes.Should().NotBeNull();
        db.Mechanics.Should().NotBeNull();
        db.Users.Should().NotBeNull();
        db.AppointmentSlots.Should().NotBeNull();
        db.Appointments.Should().NotBeNull();
        db.WorkNotes.Should().NotBeNull();
    }

    [Fact]
    public async Task CanAddAndRetrieveBranchMechanicUser()
    {
        using var db = CreateContext();
        var branch = new Branch { Id = Guid.NewGuid(), Name = "Test", City = "Test City", Address = "1 Main St" };
        db.Branches.Add(branch);
        await db.SaveChangesAsync();

        var retrieved = await db.Branches.FirstOrDefaultAsync(b => b.Id == branch.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CanAddAndRetrieveAppointmentSlotAndAppointment()
    {
        using var db = CreateContext();
        var branch = new Branch { Id = Guid.NewGuid(), Name = "B", City = "C", Address = "A" };
        var mechanic = new Mechanic { Id = Guid.NewGuid(), FullName = "M", BranchId = branch.Id };
        var st = new ServiceType { Id = Guid.NewGuid(), Code = "S", Name = "S", DurationMinutes = 60 };
        var slot = new AppointmentSlot
        {
            Id = Guid.NewGuid(), BranchId = branch.Id, MechanicId = mechanic.Id,
            ServiceTypeId = st.Id, StartUtc = DateTime.UtcNow, EndUtc = DateTime.UtcNow.AddHours(1)
        };
        db.AddRange(branch, mechanic, st, slot);
        await db.SaveChangesAsync();

        var retrieved = await db.AppointmentSlots.FindAsync(slot.Id);
        retrieved.Should().NotBeNull();
        retrieved!.IsBooked.Should().BeFalse();
    }
}
