using MechanicsSoftware.Application.UseCases.Customers.Commands;
using MechanicsSoftware.Application.UseCases.Customers.Handlers;
using MechanicsSoftware.Application.UseCases.Customers.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController( // NOSONAR S6960: Clean Architecture — each action delegates to a dedicated handler
    CreateCustomerHandler createHandler,
    DeleteCustomerHandler deleteHandler,
    GetCustomerHandler getHandler,
    ListCustomersHandler listHandler,
    UpdateCustomerHandler updateHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var result = await createHandler.ExecuteAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteHandler.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await getHandler.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListCustomersQuery query, CancellationToken cancellationToken)
    {
        var result = await listHandler.ExecuteAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        var result = await updateHandler.ExecuteAsync(id, command, cancellationToken);
        return Ok(result);
    }
}
