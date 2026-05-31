using MechanicsSoftware.Application.Abstractions;

namespace MechanicsSoftware.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private readonly int _saltRounds;

    public BCryptPasswordHasher()
    {
        _saltRounds = int.TryParse(
            Environment.GetEnvironmentVariable("BCRYPT_SALT_ROUNDS"), out var rounds) && rounds > 0
            ? rounds
            : 12;
    }

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, _saltRounds);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
