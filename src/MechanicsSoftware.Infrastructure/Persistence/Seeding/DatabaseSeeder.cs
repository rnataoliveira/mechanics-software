using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.Auth;

namespace MechanicsSoftware.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder(AppDbContext db, IPasswordHasher hasher)
{
    private const string DefaultAdminEmail = "admin@mechanics.local";
    private const string DefaultAdminPassword = "Admin@123";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL")
            ?? DefaultAdminEmail;

        var adminPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD")
            ?? DefaultAdminPassword;

        var exists = await db.Users
            .AnyAsync(u => u.Email == adminEmail, cancellationToken);

        if (exists) return;

        var admin = User.Create(
            id: Guid.NewGuid(),
            email: adminEmail,
            passwordHash: hasher.Hash(adminPassword),
            role: User.Roles.Admin);

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}
