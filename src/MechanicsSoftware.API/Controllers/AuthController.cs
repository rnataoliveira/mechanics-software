using MechanicsSoftware.API.Transport.Auth;
using MechanicsSoftware.Application.UseCases.Auth.Commands;
using MechanicsSoftware.Application.UseCases.Auth.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(LoginHandler loginHandler) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await loginHandler.ExecuteAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(result);
    }
}
