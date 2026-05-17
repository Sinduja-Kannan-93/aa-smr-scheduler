using AaSmr.Api.Data;
using AaSmr.Api.Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Tests.Data;

public class UniqueIndexTests
{
    private static AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public void ServiceType_Code_HasUniqueIndex()
    {
        using var db = CreateContext();
        var indexes = db.Model.FindEntityType(typeof(ServiceType))!.GetIndexes();
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Code") && i.IsUnique);
    }

    [Fact]
    public void AppointmentSlot_MechanicIdStartUtc_HasUniqueCompositeIndex()
    {
        using var db = CreateContext();
        var indexes = db.Model.FindEntityType(typeof(AppointmentSlot))!.GetIndexes();
        indexes.Should().Contain(i =>
            i.Properties.Count == 2 &&
            i.Properties.Any(p => p.Name == "MechanicId") &&
            i.Properties.Any(p => p.Name == "StartUtc") &&
            i.IsUnique);
    }

    [Fact]
    public void Appointment_ReferenceNumber_HasUniqueIndex()
    {
        using var db = CreateContext();
        var indexes = db.Model.FindEntityType(typeof(Appointment))!.GetIndexes();
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "ReferenceNumber") && i.IsUnique);
    }

    [Fact]
    public void Appointment_SlotId_HasUniqueIndex()
    {
        using var db = CreateContext();
        var indexes = db.Model.FindEntityType(typeof(Appointment))!.GetIndexes();
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "SlotId") && i.IsUnique);
    }

    [Fact]
    public void Appointment_Slot_DeleteBehaviorIsNoAction()
    {
        using var db = CreateContext();
        var fks = db.Model.FindEntityType(typeof(Appointment))!.GetForeignKeys();
        var slotFk = fks.Single(fk => fk.Properties.Any(p => p.Name == "SlotId"));
        slotFk.DeleteBehavior.Should().Be(DeleteBehavior.NoAction);
    }

    [Fact]
    public void WorkNote_Appointment_DeleteBehaviorIsCascade()
    {
        using var db = CreateContext();
        var fks = db.Model.FindEntityType(typeof(WorkNote))!.GetForeignKeys();
        var apptFk = fks.Single(fk => fk.Properties.Any(p => p.Name == "AppointmentId"));
        apptFk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }
}
