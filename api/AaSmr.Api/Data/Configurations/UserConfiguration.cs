using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AaSmr.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.HasOne(u => u.Mechanic)
            .WithMany()
            .HasForeignKey(u => u.MechanicId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
