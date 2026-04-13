using MechanicsSoftware.Application.Common;

namespace MechanicsSoftware.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private static readonly int SaltRounds = int.TryParse(
        Environment.GetEnvironmentVariable("BCRYPT_SALT_ROUNDS"), out var rounds) && rounds > 0
        ? rounds
        : 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, SaltRounds);

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
