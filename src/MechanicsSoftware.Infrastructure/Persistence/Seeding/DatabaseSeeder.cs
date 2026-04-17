using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Auth;

namespace MechanicsSoftware.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder(AppDbContext db, IPasswordHasher hasher)
{
    private const string DefaultAdminEmail = "admin@mechanics.local";
    private const string DefaultAdminName = "Admin";
    private const string DefaultAdminPassword = "Admin@123";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL")
            ?? DefaultAdminEmail;

        var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD")
            ?? DefaultAdminPassword;

        var exists = await db.Users
            .AnyAsync(u => u.Email == new Domain.Customers.Email(adminEmail), cancellationToken);

        if (exists) return;

        var admin = User.Create(
            id: Guid.NewGuid(),
            name: DefaultAdminName,
            email: adminEmail,
            passwordHash: hasher.Hash(adminPassword),
            role: User.Roles.Admin);

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}
