using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Auth.Commands;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Auth.Handlers;

public sealed record LoginResponse(string Token, DateTime ExpiresAt);

public sealed class LoginHandler(IAppDbContext context, IPasswordHasher hasher, IJwtProvider jwt)
{
    public async Task<LoginResponse> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var emailVo = new Email(command.Email);
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == emailVo, cancellationToken)
            ?? throw new NotFoundException("User", command.Email);

        if (!hasher.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedException();

        var token = jwt.Generate(user);
        return new LoginResponse(token.Token, token.ExpiresAt);
    }
}
