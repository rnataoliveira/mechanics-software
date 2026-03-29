using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Auth;
using MechanicsSoftware.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt);

public sealed class LoginUseCase(IAppDbContext context, IPasswordHasher hasher, IJwtProvider jwt)
{
    public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == normalizedEmail, ct)
            ?? throw new NotFoundException("User", request.Email);

        if (!hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();

        var token = jwt.Generate(user);
        return new LoginResponse(token.Token, token.ExpiresAt);
    }
}
