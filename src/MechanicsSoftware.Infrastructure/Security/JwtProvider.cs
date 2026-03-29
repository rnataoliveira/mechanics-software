using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MechanicsSoftware.Application.Common.Auth;
using MechanicsSoftware.Domain.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MechanicsSoftware.Infrastructure.Security;

public sealed class JwtProvider : IJwtProvider
{
    private readonly string _secret;
    private readonly int _expirationMinutes;

    public JwtProvider(IConfiguration configuration)
    {
        _secret = configuration["JWT_SECRET"]
            ?? throw new InvalidOperationException(
                "JWT secret not configured. Set the 'JWT_SECRET' environment variable.");

        _expirationMinutes = int.TryParse(configuration["JWT_EXPIRATION_MINUTES"], out var minutes) && minutes > 0
            ? minutes
            : 60;
    }

    public string Generate(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: ExpiresAt(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime ExpiresAt() => DateTime.UtcNow.AddMinutes(_expirationMinutes);
}
