using MechanicsSoftware.Application.Features.ServiceOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MechanicsSoftware.API.Controllers;

[ApiController]
[Route("api/service-orders")]
[Authorize]
public class ServiceOrdersController(
    CreateServiceOrderUseCase createUseCase,
    GetServiceOrderUseCase getUseCase,
    GetServiceOrderStatusUseCase getStatusUseCase,
    ListServiceOrdersUseCase listUseCase,
    StartDiagnosisUseCase startDiagnosisUseCase,
    AddServiceItemUseCase addServiceItemUseCase,
    AddPartItemUseCase addPartItemUseCase,
    GenerateBudgetUseCase generateBudgetUseCase,
    SendBudgetUseCase sendBudgetUseCase,
    ApproveServiceOrderUseCase approveUseCase,
    RejectServiceOrderUseCase rejectUseCase,
    StartExecutionUseCase startExecutionUseCase,
    CompleteServiceOrderUseCase completeUseCase,
    DeliverServiceOrderUseCase deliverUseCase,
    GetAverageExecutionTimeUseCase averageExecutionTimeUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateServiceOrderRequest request, CancellationToken ct)
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

    [HttpGet("{id:guid}/status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var result = await getStatusUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListServiceOrdersQuery query, CancellationToken ct)
    {
        var result = await listUseCase.ExecuteAsync(query, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start-diagnosis")]
    public async Task<IActionResult> StartDiagnosis(Guid id, CancellationToken ct)
    {
        var result = await startDiagnosisUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/services")]
    public async Task<IActionResult> AddService(Guid id, AddServiceItemRequest request, CancellationToken ct)
    {
        var result = await addServiceItemUseCase.ExecuteAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/parts")]
    public async Task<IActionResult> AddPart(Guid id, AddPartItemRequest request, CancellationToken ct)
    {
        var result = await addPartItemUseCase.ExecuteAsync(id, request, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/budget")]
    public async Task<IActionResult> GenerateBudget(Guid id, CancellationToken ct)
    {
        var result = await generateBudgetUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/send-budget")]
    public async Task<IActionResult> SendBudget(Guid id, CancellationToken ct)
    {
        var result = await sendBudgetUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await approveUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        var result = await rejectUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start-execution")]
    public async Task<IActionResult> StartExecution(Guid id, CancellationToken ct)
    {
        var result = await startExecutionUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var result = await completeUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var result = await deliverUseCase.ExecuteAsync(id, ct);
        return Ok(result);
    }

    [HttpGet("metrics/average-execution-time")]
    public async Task<IActionResult> AverageExecutionTime(CancellationToken ct)
    {
        var result = await averageExecutionTimeUseCase.ExecuteAsync(ct);
        return Ok(result);
    }
}
