using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AaSmr.Api.Data.Configurations;

public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
{
    public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
    {
        builder.ToTable("AppointmentSlots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.IsBooked).HasDefaultValue(false);
        builder.HasIndex(s => new { s.MechanicId, s.StartUtc }).IsUnique();
        builder.HasOne(s => s.Branch)
            .WithMany(b => b.Slots)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Mechanic)
            .WithMany()
            .HasForeignKey(s => s.MechanicId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.ServiceType)
            .WithMany()
            .HasForeignKey(s => s.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
