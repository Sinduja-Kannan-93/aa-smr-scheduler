using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db, DateTime? today = null, CancellationToken ct = default)
    {
        if (await db.Branches.AnyAsync(ct))
            return;

        var refDate = (today ?? DateTime.UtcNow).Date;

        // Branches
        var dublin = new Branch { Id = Guid.NewGuid(), Name = "Dublin", City = "Dublin", Address = "Naas Rd, Dublin 12" };
        var cork = new Branch { Id = Guid.NewGuid(), Name = "Cork", City = "Cork", Address = "Kinsale Rd, Cork" };
        var galway = new Branch { Id = Guid.NewGuid(), Name = "Galway", City = "Galway", Address = "Tuam Rd, Galway" };
        await db.Branches.AddRangeAsync([dublin, cork, galway], ct);

        // Service types
        var inspection = new ServiceType { Id = Guid.NewGuid(), Code = "Inspection", Name = "Inspection", DurationMinutes = 60 };
        var service = new ServiceType { Id = Guid.NewGuid(), Code = "Service", Name = "Service", DurationMinutes = 90 };
        var repair = new ServiceType { Id = Guid.NewGuid(), Code = "Repair", Name = "Repair", DurationMinutes = 120 };
        var diagnostics = new ServiceType { Id = Guid.NewGuid(), Code = "Diagnostics", Name = "Diagnostics", DurationMinutes = 45 };
        var serviceTypes = new[] { inspection, service, repair, diagnostics };
        await db.ServiceTypes.AddRangeAsync(serviceTypes, ct);

        // Mechanics
        var johnByrne = new Mechanic { Id = Guid.NewGuid(), FullName = "John Byrne", BranchId = dublin.Id };
        var maryOBrien = new Mechanic { Id = Guid.NewGuid(), FullName = "Mary O'Brien", BranchId = cork.Id };
        var liamWalsh = new Mechanic { Id = Guid.NewGuid(), FullName = "Liam Walsh", BranchId = galway.Id };
        var aoifeKelly = new Mechanic { Id = Guid.NewGuid(), FullName = "Aoife Kelly", BranchId = galway.Id };
        var mechanics = new[] { johnByrne, maryOBrien, liamWalsh, aoifeKelly };
        await db.Mechanics.AddRangeAsync(mechanics, ct);

        // Users
        await db.Users.AddRangeAsync([
            new User { Id = Guid.NewGuid(), FullName = "Niamh O'Sullivan", Role = UserRole.BookingAgent },
            new User { Id = Guid.NewGuid(), FullName = "John Byrne", Role = UserRole.Mechanic, MechanicId = johnByrne.Id },
            new User { Id = Guid.NewGuid(), FullName = "Mary O'Brien", Role = UserRole.Mechanic, MechanicId = maryOBrien.Id },
            new User { Id = Guid.NewGuid(), FullName = "Liam Walsh", Role = UserRole.Mechanic, MechanicId = liamWalsh.Id },
            new User { Id = Guid.NewGuid(), FullName = "Aoife Kelly", Role = UserRole.Mechanic, MechanicId = aoifeKelly.Id },
            new User { Id = Guid.NewGuid(), FullName = "Admin Manager", Role = UserRole.Admin }
        ], ct);

        // Appointment slots: 4 mechanics × 7 days × 8 hours = 224
        var slots = new List<AppointmentSlot>();
        for (int day = 0; day < 7; day++)
        {
            for (int hour = 9; hour <= 16; hour++)
            {
                int slotIndex = (day * 8) + (hour - 9);
                var st = serviceTypes[slotIndex % 4];
                foreach (var mechanic in mechanics)
                {
                    var start = refDate.AddDays(day).AddHours(hour);
                    slots.Add(new AppointmentSlot
                    {
                        Id = Guid.NewGuid(),
                        BranchId = mechanic.BranchId,
                        MechanicId = mechanic.Id,
                        ServiceTypeId = st.Id,
                        StartUtc = start,
                        EndUtc = start.AddMinutes(st.DurationMinutes),
                        IsBooked = false
                    });
                }
            }
        }
        await db.AppointmentSlots.AddRangeAsync(slots, ct);

        await db.SaveChangesAsync(ct);
    }
}
