namespace MechanicsSoftware.API.Transport.Services;

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    int BasePriceInCents,
    int EstimatedMinutes
);
