namespace AaSmr.Api.Features.Appointments;

public sealed record BookAppointmentRequest(
    Guid SlotId,
    string CustomerName,
    string CustomerPhone,
    string VehicleRegistration,
    string? Notes,
    Guid? BookingAgentUserId);
