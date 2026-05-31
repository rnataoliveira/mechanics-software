namespace MechanicsSoftware.Application.UseCases.Services.Commands;

public sealed record UpdateServiceCommand(
    string Name,
    string? Description,
    int BasePriceInCents,
    int EstimatedMinutes
);
