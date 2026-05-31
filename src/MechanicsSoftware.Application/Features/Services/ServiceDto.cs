using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Services;

public sealed record CreateServiceRequest(
    string Name,
    string? Description,
    int BasePriceInCents,
    int EstimatedMinutes
);

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    int BasePriceInCents,
    int EstimatedMinutes
);

public sealed record ListServicesQuery(
    string? Name = null
);

public sealed record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    int BasePriceInCents,
    string BasePriceFormatted,
    int EstimatedMinutes)
{
    public static ServiceResponse From(Service s) =>
        new(s.Id, s.Name, s.Description, s.BasePrice.Cents, s.BasePrice.ToFormatted(), s.EstimatedMinutes);
}
