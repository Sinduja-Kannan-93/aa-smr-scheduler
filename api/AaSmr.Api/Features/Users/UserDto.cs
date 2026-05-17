namespace AaSmr.Api.Features.Users;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Role,
    Guid? MechanicId,
    string? MechanicName,
    Guid? BranchId);
