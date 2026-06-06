using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.Abstractions;

public sealed record JwtToken(string Token, DateTime ExpiresAt);

public interface IJwtProvider
{
    JwtToken Generate(User user);
}
