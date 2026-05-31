using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using MechanicsSoftware.Application.UseCases.Auth.Commands;
using MechanicsSoftware.Application.UseCases.Auth.Handlers;
using MechanicsSoftware.IntegrationTests.Fixtures;
using MechanicsSoftware.IntegrationTests.Helpers;
using MechanicsSoftware.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicsSoftware.IntegrationTests.Base;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Class implements IAsyncLifetime which handles async disposal via xUnit")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;
    private string? _authToken;

    protected WebApplicationFactoryFixture Factory => _factory;
    protected HttpClient Client => _client;
    protected string? AuthToken => _authToken;

    protected IntegrationTestBase()
    {
        _factory = new WebApplicationFactoryFixture();
        _client = _factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await AuthenticateAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
    protected async Task AuthenticateAsync(string email = "test@example.com", string password = "Password123!")
    {
        using var loginClient = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeded = await TestDataSeeder.SeedTestUserAsync(context, email, password);

        var LoginCommand = new LoginCommand(seeded.Email, seeded.Password);
        var response = await loginClient.PostAsJsonAsync("/api/auth/login", LoginCommand);

        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to authenticate test client. Status={(int)response.StatusCode} {response.StatusCode}. Response={payload}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        _authToken = doc.RootElement.GetProperty("token").GetString();

        if (string.IsNullOrWhiteSpace(_authToken))
        {
            throw new InvalidOperationException("Authentication succeeded but JWT token was missing from response.");
        }

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
    }

    protected HttpClient CreateClientWithToken(string? token = null)
    {
        var client = _factory.CreateClient();
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }
}
