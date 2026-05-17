namespace AaSmr.Api.Domain;

public class AppointmentSlot
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public Guid MechanicId { get; set; }
    public Mechanic Mechanic { get; set; } = null!;
    public Guid ServiceTypeId { get; set; }
    public ServiceType ServiceType { get; set; } = null!;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsBooked { get; set; }
}
