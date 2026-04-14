using MechanicsSoftware.Application.Features.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController(
    CreateCustomerUseCase createUseCase, 
    DeleteCustomerUseCase deleteUseCase,
    GetCustomerUseCase getUseCase,
    ListCustomersUseCase listUseCase,
    UpdateCustomerUseCase updateUseCase ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await createUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result); 
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationtoken)
    {
        var result = await getUseCase.ExecuteAsync(id, cancellationtoken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListCustomersQuery query, CancellationToken cancellationToken) 
    {
        var result = await listUseCase.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await updateUseCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(result);
    }

}

