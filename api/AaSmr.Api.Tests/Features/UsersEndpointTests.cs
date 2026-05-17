using System.Net;
using System.Net.Http.Json;
using AaSmr.Api.Features.Users;
using AaSmr.Api.Shared;
using AaSmr.Api.Tests.Infrastructure;
using FluentAssertions;

namespace AaSmr.Api.Tests.Features;

public class UsersEndpointTests : IDisposable
{
    private readonly InMemoryWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public UsersEndpointTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetUsers_Returns200WithAllSeedUsers()
    {
        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content
            .ReadFromJsonAsync<ApiResponse<List<UserDto>>>();
        body!.Success.Should().BeTrue();
        body.Data.Should().HaveCount(6);
    }

    [Fact]
    public async Task GetUsers_IncludesBookingAgentAndMechanicAndAdmin()
    {
        var body = await _client.GetFromJsonAsync<ApiResponse<List<UserDto>>>("/api/users");

        var roles = body!.Data!.Select(u => u.Role).Distinct().ToList();
        roles.Should().Contain("BookingAgent");
        roles.Should().Contain("Mechanic");
        roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task GetUsers_MechanicsHaveMechanicIdAndBranchId()
    {
        var body = await _client.GetFromJsonAsync<ApiResponse<List<UserDto>>>("/api/users");

        var mechanics = body!.Data!.Where(u => u.Role == "Mechanic").ToList();
        mechanics.Should().HaveCount(4);
        mechanics.Should().AllSatisfy(u =>
        {
            u.MechanicId.Should().NotBeNull();
            u.BranchId.Should().NotBeNull();
        });
    }
}
