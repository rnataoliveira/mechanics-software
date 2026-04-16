using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MechanicsSoftware.Infrastructure.Persistence;
using Npgsql;

namespace MechanicsSoftware.IntegrationTests.Fixtures;

public sealed class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    private static string CreateConnectionString()
        => $"Server=localhost;Port=5432;Database=mechanics_software_test;User Id=postgres;Password=postgres;";

    private readonly string _connectionString = CreateConnectionString();
    private const string _jwtSecret = "test-secret-key-for-testing-only-min-32-chars-required-here";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });

        builder.UseEnvironment("Test");

        Environment.SetEnvironmentVariable("JWT_SECRET", _jwtSecret);
        Environment.SetEnvironmentVariable("JWT_EXPIRATION_MINUTES", "60");
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await EnsureDatabaseExistsAsync();

        try
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            await context.Database.EnsureCreatedAsync();
        }
    }

    private async Task EnsureDatabaseExistsAsync()
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;

        var maintenanceCsb = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = "postgres"
        };

        await using var conn = new NpgsqlConnection(maintenanceCsb.ConnectionString);
        await conn.OpenAsync();

        await using (var existsCmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", conn))
        {
            existsCmd.Parameters.AddWithValue("name", databaseName);
            var exists = await existsCmd.ExecuteScalarAsync();
            if (exists is not null)
                return;
        }

        await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", conn);
        await createCmd.ExecuteNonQueryAsync();
    }
}
