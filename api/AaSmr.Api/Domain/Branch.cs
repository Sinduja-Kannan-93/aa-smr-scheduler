namespace AaSmr.Api.Domain;

public class Branch
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public ICollection<Mechanic> Mechanics { get; set; } = [];
    public ICollection<AppointmentSlot> Slots { get; set; } = [];
}
