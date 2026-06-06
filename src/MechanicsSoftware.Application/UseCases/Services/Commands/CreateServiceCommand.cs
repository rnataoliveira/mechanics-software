namespace MechanicsSoftware.Application.UseCases.Services.Commands;

public sealed record CreateServiceCommand(
    string Name,
    string? Description,
    int BasePriceInCents,
    int EstimatedMinutes
);
