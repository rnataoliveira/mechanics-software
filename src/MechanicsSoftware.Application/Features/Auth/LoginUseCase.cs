using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Auth;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, DateTime ExpiresAt);

public sealed class LoginUseCase(IAppDbContext context, IPasswordHasher hasher, IJwtProvider jwt)
{
    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var emailVo = new Email(request.Email);
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == emailVo, cancellationToken)
            ?? throw new NotFoundException("User", request.Email);

        if (!hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();

        var token = jwt.Generate(user);
        return new LoginResponse(token.Token, token.ExpiresAt);
    }
}
