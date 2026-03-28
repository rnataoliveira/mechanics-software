using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Auth;

public sealed class User : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    private User() { }

    public static User Create(Guid id, string name, string email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("User name is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User
        {
            Id = id,
            Name = name.Trim(),
            Email = new Email(email),
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string Attendant = "ATTENDANT";
        public const string Mechanic = "MECHANIC";
    }
}
