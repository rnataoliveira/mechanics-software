using MechanicsSoftware.Domain.Auth;

namespace MechanicsSoftware.Application.Common.Auth;

public sealed record JwtToken(string Token, DateTime ExpiresAt);

public interface IJwtProvider
{
    JwtToken Generate(User user);
}
