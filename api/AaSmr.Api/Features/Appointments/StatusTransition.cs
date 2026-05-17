using AaSmr.Api.Domain;

namespace AaSmr.Api.Features.Appointments;

public static class StatusTransition
{
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> Allowed = new()
    {
        [AppointmentStatus.Scheduled]  = [AppointmentStatus.InProgress, AppointmentStatus.NoShow],
        [AppointmentStatus.InProgress] = [AppointmentStatus.Completed],
        [AppointmentStatus.Completed]  = [],
        [AppointmentStatus.NoShow]     = []
    };

    public static bool IsValid(AppointmentStatus from, AppointmentStatus to) =>
        Allowed.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
