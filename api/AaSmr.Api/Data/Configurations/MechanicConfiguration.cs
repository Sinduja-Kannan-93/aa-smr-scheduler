using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AaSmr.Api.Data.Configurations;

public class MechanicConfiguration : IEntityTypeConfiguration<Mechanic>
{
    public void Configure(EntityTypeBuilder<Mechanic> builder)
    {
        builder.ToTable("Mechanics");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FullName).HasMaxLength(200).IsRequired();
        builder.HasOne(m => m.Branch)
            .WithMany(b => b.Mechanics)
            .HasForeignKey(m => m.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
