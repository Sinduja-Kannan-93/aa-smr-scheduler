using System.Net;
using System.Net.Http.Json;
using AaSmr.Api.Data;
using AaSmr.Api.Features.Appointments;
using AaSmr.Api.Features.Slots;
using AaSmr.Api.Shared;
using AaSmr.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AaSmr.Api.Tests.Features;

public class AppointmentsEndpointTests : IDisposable
{
    private readonly InMemoryWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public AppointmentsEndpointTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<SlotDto> GetFirstAvailableSlotAsync()
    {
        var body = await _client.GetFromJsonAsync<ApiResponse<List<SlotDto>>>("/api/slots");
        return body!.Data!.First();
    }

    private static BookAppointmentRequest ValidRequest(Guid slotId) =>
        new(slotId, "Mary Murphy", "0861234567", "201-D-12345", "Oil change please", null);

    // ── Book appointment ─────────────────────────────────────────────────────

    [Fact]
    public async Task BookAppointment_HappyPath_Returns200WithReferenceNumber()
    {
        var slot = await GetFirstAvailableSlotAsync();

        var response = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>();
        body!.Success.Should().BeTrue();
        body.Data!.ReferenceNumber.Should().MatchRegex(@"^SMR-\d{4}-[A-Z2-9]{6}$");
    }

    [Fact]
    public async Task BookAppointment_SlotMarkedBookedAfterSuccess()
    {
        var slot = await GetFirstAvailableSlotAsync();
        await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbSlot = await db.AppointmentSlots.FindAsync(slot.Id);
        dbSlot!.IsBooked.Should().BeTrue();
    }

    [Fact]
    public async Task BookAppointment_SecondBookingOfSameSlot_Returns409()
    {
        var slot = await GetFirstAvailableSlotAsync();
        await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));

        var second = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await second.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
        body.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BookAppointment_MissingFields_Returns400()
    {
        var badRequest = new BookAppointmentRequest(Guid.NewGuid(), "", "", "", null, null);

        var response = await _client.PostAsJsonAsync("/api/appointments", badRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task BookAppointment_UnknownSlot_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(Guid.NewGuid()));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BookAppointment_VehicleRegIsUppercased()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var request = new BookAppointmentRequest(slot.Id, "Test", "0861234567", "201-d-99999", null, null);

        var bookResp = await _client.PostAsJsonAsync("/api/appointments", request);
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        var detail = await _client.GetFromJsonAsync<ApiResponse<AppointmentDetailDto>>(
            $"/api/appointments/{booked.Id}");
        detail!.Data!.VehicleRegistration.Should().Be("201-D-99999");
    }

    // ── Appointment detail ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAppointmentById_Returns200WithDetail()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        var response = await _client.GetAsync($"/api/appointments/{booked.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AppointmentDetailDto>>();
        body!.Data!.ReferenceNumber.Should().Be(booked.ReferenceNumber);
        body.Data.Status.Should().Be("Scheduled");
        body.Data.WorkNotes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/appointments/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Today's appointments ─────────────────────────────────────────────────

    [Fact]
    public async Task GetTodayAppointments_Returns200()
    {
        var response = await _client.GetAsync("/api/appointments/today");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Work notes ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AddWorkNote_Returns200WithNoteDto()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mechanic = db.Mechanics.First();

        var response = await _client.PostAsJsonAsync(
            $"/api/appointments/{booked.Id}/notes",
            new AddWorkNoteRequest("Oil filter replaced", mechanic.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<WorkNoteDto>>();
        body!.Data!.Body.Should().Be("Oil filter replaced");
        body.Data.AuthorName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AddWorkNote_EmptyBody_Returns400()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mechanic = db.Mechanics.First();

        var response = await _client.PostAsJsonAsync(
            $"/api/appointments/{booked.Id}/notes",
            new AddWorkNoteRequest("", mechanic.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddWorkNote_AppearsInDetailWorkNotes()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mechanic = db.Mechanics.First();

        await _client.PostAsJsonAsync(
            $"/api/appointments/{booked.Id}/notes",
            new AddWorkNoteRequest("Checked brakes", mechanic.Id));

        var detail = await _client.GetFromJsonAsync<ApiResponse<AppointmentDetailDto>>(
            $"/api/appointments/{booked.Id}");
        detail!.Data!.WorkNotes.Should().HaveCount(1);
        detail.Data.WorkNotes[0].Body.Should().Be("Checked brakes");
    }

    // ── Status transitions ───────────────────────────────────────────────────

    [Fact]
    public async Task PatchStatus_ScheduledToInProgress_Returns200()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        var response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{booked.Id}/status",
            new PatchStatusRequest("InProgress"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PatchStatus_InProgressToCompleted_Returns200()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        await _client.PatchAsJsonAsync(
            $"/api/appointments/{booked.Id}/status", new PatchStatusRequest("InProgress"));
        var response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{booked.Id}/status", new PatchStatusRequest("Completed"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PatchStatus_IllegalTransition_ScheduledToCompleted_Returns400()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        var response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{booked.Id}/status", new PatchStatusRequest("Completed"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        body!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PatchStatus_InvalidStatusString_Returns400()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        var response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{booked.Id}/status", new PatchStatusRequest("Cancelled"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PatchStatus_ScheduledToNoShow_Returns200()
    {
        var slot = await GetFirstAvailableSlotAsync();
        var bookResp = await _client.PostAsJsonAsync("/api/appointments", ValidRequest(slot.Id));
        var booked = (await bookResp.Content.ReadFromJsonAsync<ApiResponse<BookAppointmentResponse>>())!.Data!;

        var response = await _client.PatchAsJsonAsync(
            $"/api/appointments/{booked.Id}/status", new PatchStatusRequest("NoShow"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
