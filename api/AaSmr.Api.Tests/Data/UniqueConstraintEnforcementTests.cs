using AaSmr.Api.Domain;
using AaSmr.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Tests.Data;

public class UniqueConstraintEnforcementTests
{
    [Fact]
    public async Task InsertingDuplicateServiceTypeCode_ThrowsDbUpdateException()
    {
        using var db = SqliteAppDbContextFactory.Create();
        db.ServiceTypes.AddRange(
            new ServiceType { Id = Guid.NewGuid(), Code = "DUP", Name = "A", DurationMinutes = 60 },
            new ServiceType { Id = Guid.NewGuid(), Code = "DUP", Name = "B", DurationMinutes = 60 }
        );
        await db.Invoking(d => d.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task InsertingDuplicateAppointmentSlot_SameMechanicAndStart_ThrowsDbUpdateException()
    {
        using var db = SqliteAppDbContextFactory.Create();
        var mechId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var stId = Guid.NewGuid();
        var start = DateTime.UtcNow;
        db.Branches.Add(new AaSmr.Api.Domain.Branch { Id = branchId, Name = "B", City = "C", Address = "A" });
        db.Mechanics.Add(new Mechanic { Id = mechId, FullName = "M", BranchId = branchId });
        db.ServiceTypes.Add(new ServiceType { Id = stId, Code = "X", Name = "X", DurationMinutes = 60 });
        await db.SaveChangesAsync();

        db.AppointmentSlots.AddRange(
            new AppointmentSlot { Id = Guid.NewGuid(), BranchId = branchId, MechanicId = mechId, ServiceTypeId = stId, StartUtc = start, EndUtc = start.AddHours(1) },
            new AppointmentSlot { Id = Guid.NewGuid(), BranchId = branchId, MechanicId = mechId, ServiceTypeId = stId, StartUtc = start, EndUtc = start.AddHours(1) }
        );
        await db.Invoking(d => d.SaveChangesAsync()).Should().ThrowAsync<DbUpdateException>();
    }
}
