using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MechanicsSoftware.API.Transport.Auth;
using MechanicsSoftware.IntegrationTests.Base;
using MechanicsSoftware.IntegrationTests.Fixtures;
using MechanicsSoftware.IntegrationTests.Helpers;
using MechanicsSoftware.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicsSoftware.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class AuthIntegrationTests : IntegrationTestBase
{
    private const string ValidEmail = "admin@mechanics.com";
    private const string ValidPassword = "SecurePassword123!";
    private const string InvalidPassword = "WrongPassword";

    public AuthIntegrationTests(WebApplicationFactoryFixture factory) : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    private HttpClient GetUnauthenticatedClient() => Factory.CreateClient();

    [Fact]
    public async Task LoginWithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var client = GetUnauthenticatedClient();
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDataSeeder.SeedTestUserAsync(context, ValidEmail, ValidPassword);

        var request = new LoginRequest(ValidEmail, ValidPassword);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var token = doc.RootElement.GetProperty("token").GetString();
        var expiresAt = doc.RootElement.GetProperty("expiresAt").GetDateTime();

        token.Should().NotBeNullOrEmpty();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginWithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var client = GetUnauthenticatedClient();
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDataSeeder.SeedTestUserAsync(context, ValidEmail, ValidPassword);

        var request = new LoginRequest(ValidEmail, InvalidPassword);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginWithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var client = GetUnauthenticatedClient();
        var request = new LoginRequest("nonexistent@example.com", ValidPassword);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
