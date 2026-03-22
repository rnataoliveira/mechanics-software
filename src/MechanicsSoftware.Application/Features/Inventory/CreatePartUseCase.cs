using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class CreatePartUseCase(IPartRepository repository)
{
    public async Task<PartOutput> ExecuteAsync(CreatePartInput input, CancellationToken ct = default)
    {
        var existing = await repository.GetByCodeAsync(input.Code, ct);
        if (existing is not null)
            throw new DomainException($"A part with code '{input.Code}' already exists.");

        var part = Part.Create(
            Guid.NewGuid(),
            input.Code,
            input.Name,
            input.Description,
            new Money(input.UnitPriceInCents),
            input.InitialStock
        );

        await repository.AddAsync(part, ct);
        return PartOutput.From(part);
    }
}
