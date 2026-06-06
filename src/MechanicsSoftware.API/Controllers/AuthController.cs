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
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await loginHandler.ExecuteAsync(command, cancellationToken);
        return Ok(result);
    }
}
