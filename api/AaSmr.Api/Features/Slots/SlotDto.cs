namespace AaSmr.Api.Features.Slots;

public sealed record SlotDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    Guid MechanicId,
    string MechanicName,
    Guid ServiceTypeId,
    string ServiceTypeName,
    int DurationMinutes,
    DateTime StartUtc,
    DateTime EndUtc);
