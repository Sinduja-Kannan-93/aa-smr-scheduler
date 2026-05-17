using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AaSmr.Api.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ReferenceNumber).HasMaxLength(32).IsRequired();
        builder.HasIndex(a => a.ReferenceNumber).IsUnique();
        builder.HasIndex(a => a.SlotId).IsUnique();
        builder.Property(a => a.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.CustomerPhone).HasMaxLength(40).IsRequired();
        builder.Property(a => a.VehicleRegistration).HasMaxLength(16).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.Status).HasDefaultValue(AppointmentStatus.Scheduled);
        builder.HasOne(a => a.Slot)
            .WithOne()
            .HasForeignKey<Appointment>(a => a.SlotId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(a => a.BookingAgentUser)
            .WithMany()
            .HasForeignKey(a => a.BookingAgentUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
