using MechanicsSoftware.Application.Features.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController( // NOSONAR S6960: VSA — each action delegates to a dedicated use case
    CreateServiceUseCase createUseCase,
    GetServiceUseCase getUseCase,
    ListServicesUseCase listUseCase,
    UpdateServiceUseCase updateUseCase,
    DeleteServiceUseCase deleteUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await createUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await getUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListServicesQuery query, CancellationToken cancellationToken)
    {
        var result = await listUseCase.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await updateUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
