namespace AaSmr.Api.Features.Appointments;

public sealed record WorkNoteDto(
    Guid Id,
    string Body,
    string AuthorName,
    DateTime CreatedUtc);

public sealed record AppointmentListItemDto(
    Guid Id,
    string ReferenceNumber,
    string CustomerName,
    string CustomerPhone,
    string VehicleRegistration,
    string Status,
    DateTime StartUtc,
    DateTime EndUtc,
    string ServiceTypeName,
    string MechanicName,
    string BranchName);

public sealed record AppointmentDetailDto(
    Guid Id,
    string ReferenceNumber,
    string CustomerName,
    string CustomerPhone,
    string VehicleRegistration,
    string? Notes,
    string Status,
    DateTime CreatedUtc,
    DateTime StartUtc,
    DateTime EndUtc,
    string ServiceTypeName,
    string MechanicName,
    string BranchName,
    IReadOnlyList<WorkNoteDto> WorkNotes);

public sealed record MechanicScheduleDto(
    Guid MechanicId,
    string MechanicName,
    string BranchName,
    IReadOnlyList<AppointmentListItemDto> Appointments);

public sealed record BookAppointmentResponse(Guid Id, string ReferenceNumber);
