using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Services;

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
