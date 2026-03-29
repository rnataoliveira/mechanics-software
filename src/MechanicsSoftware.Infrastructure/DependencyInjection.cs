using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Auth;
using MechanicsSoftware.Infrastructure.Persistence;
using MechanicsSoftware.Infrastructure.Persistence.Seeding;
using MechanicsSoftware.Infrastructure.Security;

namespace MechanicsSoftware.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException(
                "Connection string not found. Set 'ConnectionStrings:DefaultConnection' or 'DATABASE_URL'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
