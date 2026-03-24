using MechanicsSoftware.Domain.Auth;

namespace MechanicsSoftware.Application.Common.Auth;

public interface IJwtProvider
{
    string Generate(User user);
    DateTime ExpiresAt();
}
