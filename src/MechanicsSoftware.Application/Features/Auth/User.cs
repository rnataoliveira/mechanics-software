namespace MechanicsSoftware.Application.Features.Auth;

/// <summary>
/// Persistence model for application users. This is NOT a domain aggregate —
/// authentication is an infrastructure/security concern. Lives in Application
/// so IAppDbContext can reference it without a circular dependency.
/// </summary>
public sealed class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    private User() { } // EF Core

    public static User Create(Guid id, string email, string passwordHash, string role) =>
        new() { Id = id, Email = email, PasswordHash = passwordHash, Role = role };

    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string Attendant = "ATTENDANT";
        public const string Mechanic = "MECHANIC";
    }
}
