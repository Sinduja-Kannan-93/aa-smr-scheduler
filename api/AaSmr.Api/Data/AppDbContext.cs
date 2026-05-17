using AaSmr.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AaSmr.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Mechanic> Mechanics => Set<Mechanic>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<WorkNote> WorkNotes => Set<WorkNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
