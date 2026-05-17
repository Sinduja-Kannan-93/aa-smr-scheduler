using System.Net;
using System.Net.Http.Json;
using AaSmr.Api.Data;
using AaSmr.Api.Features.Slots;
using AaSmr.Api.Shared;
using AaSmr.Api.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AaSmr.Api.Tests.Features;

public class SlotsEndpointTests : IDisposable
{
    private readonly InMemoryWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public SlotsEndpointTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetSlots_Returns200()
    {
        var response = await _client.GetAsync("/api/slots");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSlots_DefaultWindow_ReturnsUnbookedSlots()
    {
        var body = await _client.GetFromJsonAsync<ApiResponse<List<SlotDto>>>("/api/slots");

        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSlots_FilterByServiceType_ReturnsOnlyMatchingSlots()
    {
        var stBody = await _client.GetFromJsonAsync<ApiResponse<List<AaSmr.Api.Features.ServiceTypes.ServiceTypeDto>>>(
            "/api/service-types");
        var stId = stBody!.Data!.First().Id;

        var body = await _client.GetFromJsonAsync<ApiResponse<List<SlotDto>>>(
            $"/api/slots?serviceTypeId={stId}");

        body!.Data.Should().NotBeEmpty();
        body.Data!.Should().AllSatisfy(s => s.ServiceTypeId.Should().Be(stId));
    }

    [Fact]
    public async Task GetSlots_FilterByBranch_ReturnsOnlyMatchingSlots()
    {
        var branchBody = await _client.GetFromJsonAsync<ApiResponse<List<AaSmr.Api.Features.Branches.BranchDto>>>(
            "/api/branches");
        var branchId = branchBody!.Data!.First().Id;

        var body = await _client.GetFromJsonAsync<ApiResponse<List<SlotDto>>>(
            $"/api/slots?branchId={branchId}");

        body!.Data.Should().NotBeEmpty();
        body.Data!.Should().AllSatisfy(s => s.BranchId.Should().Be(branchId));
    }

    [Fact]
    public async Task GetSlots_ExcludesBookedSlots()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slot = db.AppointmentSlots.First();
        var bookedSlotId = slot.Id;
        slot.IsBooked = true;
        await db.SaveChangesAsync();

        var body = await _client.GetFromJsonAsync<ApiResponse<List<SlotDto>>>("/api/slots");

        body!.Data!.Should().NotContain(s => s.Id == bookedSlotId);
    }

    [Fact]
    public async Task GetSlots_OrderedByStartUtc()
    {
        var body = await _client.GetFromJsonAsync<ApiResponse<List<SlotDto>>>("/api/slots");

        var times = body!.Data!.Select(s => s.StartUtc).ToList();
        times.Should().BeInAscendingOrder();
    }
}
