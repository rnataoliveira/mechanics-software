namespace MechanicsSoftware.Application.Common.Auth;

public interface IPasswordHasher
{
    bool Verify(string plainPassword, string hash);
}
