namespace AaSmr.Api.Domain;

public class Appointment
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid SlotId { get; set; }
    public AppointmentSlot Slot { get; set; } = null!;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime CreatedUtc { get; set; }
    public Guid? BookingAgentUserId { get; set; }
    public User? BookingAgentUser { get; set; }
    public ICollection<WorkNote> WorkNotes { get; set; } = [];
}
