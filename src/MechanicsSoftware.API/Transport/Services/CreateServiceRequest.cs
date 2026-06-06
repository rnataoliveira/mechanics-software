namespace MechanicsSoftware.API.Transport.Services;

public sealed record CreateServiceRequest(
    string Name,
    string? Description,
    int BasePriceInCents,
    int EstimatedMinutes
);
