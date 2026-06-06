using MechanicsSoftware.Application.UseCases.Vehicles.Commands;
using MechanicsSoftware.Application.UseCases.Vehicles.Handlers;
using MechanicsSoftware.Application.UseCases.Vehicles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController(CreateVehicleHandler createVehicle, // NOSONAR S6960: Clean Architecture — each action delegates to a dedicated handler
    DeleteVehicleHandler deleteVehicle,
    GetVehicleHandler getVehicle,
    ListVehiclesHandler listVehicles,
    UpdateVehicleHandler updateVehicle) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateVehicleCommand command, CancellationToken cancellationToken)
    {
        var result = await createVehicle.ExecuteAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteVehicle.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await getVehicle.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListVehiclesQuery query, CancellationToken cancellationToken)
    {
        var result = await listVehicles.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVehicleCommand command, CancellationToken cancellationToken)
    {
        var result = await updateVehicle.ExecuteAsync(command with { Id = id }, cancellationToken);
        return Ok(result);
    }
}
