using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MechanicsSoftware.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL") ??
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            "Host=localhost;Port=5435;Database=mechanics_software;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
