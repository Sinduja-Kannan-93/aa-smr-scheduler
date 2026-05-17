namespace AaSmr.Api.Domain;

public class WorkNote
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string Body { get; set; } = string.Empty;
    public Guid AuthorMechanicId { get; set; }
    public Mechanic AuthorMechanic { get; set; } = null!;
    public DateTime CreatedUtc { get; set; }
}
