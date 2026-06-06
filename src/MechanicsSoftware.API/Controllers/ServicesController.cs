using MechanicsSoftware.Application.UseCases.Services.Commands;
using MechanicsSoftware.Application.UseCases.Services.Handlers;
using MechanicsSoftware.Application.UseCases.Services.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController( // NOSONAR S6960: Clean Architecture — each action delegates to a dedicated handler
    CreateServiceHandler createHandler,
    GetServiceHandler getHandler,
    ListServicesHandler listHandler,
    UpdateServiceHandler updateHandler,
    DeleteServiceHandler deleteHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        var result = await createHandler.ExecuteAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await getHandler.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListServicesQuery query, CancellationToken cancellationToken)
    {
        var result = await listHandler.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        var result = await updateHandler.ExecuteAsync(id, command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteHandler.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
