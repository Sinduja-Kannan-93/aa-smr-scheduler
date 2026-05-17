using AaSmr.Api.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AaSmr.Api.Tests.Integration;

public class StartupSeedIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = Guid.NewGuid().ToString();

    public StartupSeedIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(o =>
                    o.UseInMemoryDatabase(_dbName));
            });
        });
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task HealthEndpoint_StillReturns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Startup_SeedsBranches()
    {
        _ = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var count = await db.Branches.CountAsync();
        count.Should().Be(3);
    }
}
