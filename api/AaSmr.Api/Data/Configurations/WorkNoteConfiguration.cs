using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AaSmr.Api.Data.Configurations;

public class WorkNoteConfiguration : IEntityTypeConfiguration<WorkNote>
{
    public void Configure(EntityTypeBuilder<WorkNote> builder)
    {
        builder.ToTable("WorkNotes");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Body).HasMaxLength(2000).IsRequired();
        builder.HasOne(w => w.Appointment)
            .WithMany(a => a.WorkNotes)
            .HasForeignKey(w => w.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(w => w.AuthorMechanic)
            .WithMany(m => m.WorkNotes)
            .HasForeignKey(w => w.AuthorMechanicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
