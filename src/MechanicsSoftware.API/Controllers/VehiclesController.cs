using MechanicsSoftware.Application.Features.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController(CreateVehicleUseCase createVehicle, // NOSONAR S6960: VSA — each action delegates to a dedicated use case
    DeleteVehicleUseCase deleteVehicle,
    GetVehicleUseCase getVehicle,
    ListVehiclesUseCase listVehicles,
    UpdateVehicleUseCase updateVehicle) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Create(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await createVehicle.ExecuteAsync(request, cancellationToken);
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
    public async Task<IActionResult> Update(Guid id, UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        var result = await updateVehicle.ExecuteAsync(request with { Id = id}, cancellationToken);
        return Ok(result);
    }
}
