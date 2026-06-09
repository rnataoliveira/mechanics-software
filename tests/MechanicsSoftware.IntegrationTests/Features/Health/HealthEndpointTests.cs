using System.Net;
using FluentAssertions;
using MechanicsSoftware.IntegrationTests.Infrastructure;

namespace MechanicsSoftware.IntegrationTests.Features.Health;

public sealed class HealthEndpointTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public HealthEndpointTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Health_WithoutAuth_ReturnsOk()
    {
        var response = await _factory.CreateClient().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
