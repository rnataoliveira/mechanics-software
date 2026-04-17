using MechanicsSoftware.Application.Features.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController(
    CreateServiceUseCase createUseCase,
    GetServiceUseCase getUseCase,
    ListServicesUseCase listUseCase,
    UpdateServiceUseCase updateUseCase,
    DeleteServiceUseCase deleteUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceRequest request, CancellationToken ct)
    {
        var result = await createUseCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await getUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListServicesQuery query, CancellationToken ct)
    {
        var result = await listUseCase.ExecuteAsync(query, ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateServiceRequest request, CancellationToken ct)
    {
        var result = await updateUseCase.ExecuteAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await deleteUseCase.ExecuteAsync(id, ct);
        return NoContent();
    }
}
