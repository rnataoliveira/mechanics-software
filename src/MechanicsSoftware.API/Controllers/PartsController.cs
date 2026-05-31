using MechanicsSoftware.Application.UseCases.Inventory.Commands;
using MechanicsSoftware.Application.UseCases.Inventory.Handlers;
using MechanicsSoftware.Application.UseCases.Inventory.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize]
public class PartsController(CreatePartHandler createPart, // NOSONAR S6960: Clean Architecture — each action delegates to a dedicated handler
    DeletePartHandler deletePart,
    GetPartHandler getPart,
    ListPartsHandler listParts,
    UpdatePartHandler updatePart,
    UpdateStockHandler updateStock) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePartCommand command, CancellationToken cancellationToken)
    {
        var result = await createPart.ExecuteAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deletePart.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await getPart.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListPartsQuery query, CancellationToken cancellationToken)
    {
        var result = await listParts.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePartCommand command, CancellationToken cancellationToken)
    {
        var result = await updatePart.ExecuteAsync(id, command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, UpdateStockCommand command, CancellationToken cancellationToken)
    {
        var result = await updateStock.ExecuteAsync(id, command, cancellationToken);
        return Ok(result);
    }
}
