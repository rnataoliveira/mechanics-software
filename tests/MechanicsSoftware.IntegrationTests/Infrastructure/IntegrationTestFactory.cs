using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace MechanicsSoftware.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresHost = "localhost";
    private const string PostgresPort = "5435";
    private const string PostgresUser = "postgres";
    private const string PostgresPassword = "postgres";
    private const string AdminDatabase = "postgres";

    private readonly string _testDatabaseName = $"mechanics_software_it_{Guid.NewGuid():N}";

    public async Task InitializeAsync()
    {
        await ExecuteAdminCommandAsync($"CREATE DATABASE \"{_testDatabaseName}\"");
        // Boot the host so Program.cs runs migrations and admin seeding against the new database.
        _ = Server;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await ExecuteAdminCommandAsync(
            $"DROP DATABASE IF EXISTS \"{_testDatabaseName}\" WITH (FORCE)");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);
        builder.UseSetting("JWT_SECRET", "integration-tests-secret-min-32-chars!!");
    }

    private string TestConnectionString =>
        $"Host={PostgresHost};Port={PostgresPort};Database={_testDatabaseName};" +
        $"Username={PostgresUser};Password={PostgresPassword}";

    private static string AdminConnectionString =>
        $"Host={PostgresHost};Port={PostgresPort};Database={AdminDatabase};" +
        $"Username={PostgresUser};Password={PostgresPassword}";

    private static async Task ExecuteAdminCommandAsync(string commandText)
    {
        await using var connection = new NpgsqlConnection(AdminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }
}
