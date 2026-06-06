using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Common.Auth;

public sealed record JwtToken(string Token, DateTime ExpiresAt);

public interface IJwtProvider
{
    JwtToken Generate(User user);
}
