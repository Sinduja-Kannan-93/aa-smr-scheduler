using AaSmr.Api.Domain;
using AaSmr.Api.Features.Appointments;
using FluentAssertions;

namespace AaSmr.Api.Tests.Features;

public class StatusTransitionTests
{
    [Theory]
    [InlineData(AppointmentStatus.Scheduled,  AppointmentStatus.InProgress, true)]
    [InlineData(AppointmentStatus.Scheduled,  AppointmentStatus.NoShow,     true)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Completed,  true)]
    [InlineData(AppointmentStatus.Scheduled,  AppointmentStatus.Completed,  false)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.NoShow,     false)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Scheduled,  false)]
    [InlineData(AppointmentStatus.Completed,  AppointmentStatus.InProgress, false)]
    [InlineData(AppointmentStatus.NoShow,     AppointmentStatus.Scheduled,  false)]
    [InlineData(AppointmentStatus.Completed,  AppointmentStatus.Completed,  false)]
    [InlineData(AppointmentStatus.NoShow,     AppointmentStatus.NoShow,     false)]
    public void IsValid_ReturnsExpected(
        AppointmentStatus from, AppointmentStatus to, bool expected)
    {
        StatusTransition.IsValid(from, to).Should().Be(expected);
    }
}
