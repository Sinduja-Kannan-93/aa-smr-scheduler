using AaSmr.Api.Domain;
using FluentAssertions;

namespace AaSmr.Api.Tests.Domain;

public class EntityShapeTests
{
    [Fact]
    public void Branch_ExposesIdNameCityAddress()
    {
        var b = new Branch { Id = Guid.NewGuid(), Name = "Dublin", City = "Dublin", Address = "Naas Rd" };
        b.Name.Should().Be("Dublin");
        b.City.Should().Be("Dublin");
        b.Address.Should().Be("Naas Rd");
    }

    [Fact]
    public void ServiceType_ExposesIdCodeNameDurationMinutes()
    {
        var st = new ServiceType { Id = Guid.NewGuid(), Code = "Inspection", Name = "Inspection", DurationMinutes = 60 };
        st.Code.Should().Be("Inspection");
        st.DurationMinutes.Should().Be(60);
    }

    [Fact]
    public void Mechanic_ExposesIdFullNameBranchId()
    {
        var branchId = Guid.NewGuid();
        var m = new Mechanic { Id = Guid.NewGuid(), FullName = "John Byrne", BranchId = branchId };
        m.FullName.Should().Be("John Byrne");
        m.BranchId.Should().Be(branchId);
    }

    [Fact]
    public void User_ExposesIdFullNameRoleAndNullableMechanicId()
    {
        var u = new User { Id = Guid.NewGuid(), FullName = "Niamh", Role = UserRole.BookingAgent };
        u.MechanicId.Should().BeNull();
        u.Role.Should().Be(UserRole.BookingAgent);
    }

    [Fact]
    public void AppointmentSlot_ExposesAllFieldsWithIsBookedDefaultFalse()
    {
        var slot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            MechanicId = Guid.NewGuid(),
            ServiceTypeId = Guid.NewGuid(),
            StartUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddHours(1)
        };
        slot.IsBooked.Should().BeFalse();
    }

    [Fact]
    public void Appointment_ExposesAllFieldsAndDefaultStatusScheduled()
    {
        var a = new Appointment
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = "SMR-2026-ABCDEF",
            SlotId = Guid.NewGuid(),
            CustomerName = "Jane Doe",
            CustomerPhone = "+353 87 123 4567",
            VehicleRegistration = "24-D-12345",
            CreatedUtc = DateTime.UtcNow
        };
        a.Status.Should().Be(AppointmentStatus.Scheduled);
        a.Notes.Should().BeNull();
        a.BookingAgentUserId.Should().BeNull();
    }

    [Fact]
    public void WorkNote_ExposesIdAppointmentIdBodyAuthorMechanicIdCreatedUtc()
    {
        var note = new WorkNote
        {
            Id = Guid.NewGuid(),
            AppointmentId = Guid.NewGuid(),
            Body = "Replaced brake pads",
            AuthorMechanicId = Guid.NewGuid(),
            CreatedUtc = DateTime.UtcNow
        };
        note.Body.Should().Be("Replaced brake pads");
    }

    [Fact]
    public void AppointmentStatus_HasFourMembers()
    {
        var values = Enum.GetValues<AppointmentStatus>();
        values.Should().Contain(AppointmentStatus.Scheduled);
        values.Should().Contain(AppointmentStatus.InProgress);
        values.Should().Contain(AppointmentStatus.Completed);
        values.Should().Contain(AppointmentStatus.NoShow);
        values.Should().HaveCount(4);
    }

    [Fact]
    public void UserRole_HasThreeMembers()
    {
        var values = Enum.GetValues<UserRole>();
        values.Should().Contain(UserRole.BookingAgent);
        values.Should().Contain(UserRole.Mechanic);
        values.Should().Contain(UserRole.Admin);
        values.Should().HaveCount(3);
    }
}
