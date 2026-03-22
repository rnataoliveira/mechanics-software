using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class UpdatePartUseCase(IPartRepository repository)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdatePartInput input,
                                               CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct)
                   ?? throw new DomainException($"Part with id '{id}' not found.");

        part.Update(input.Name, input.Description, new Money(input.UnitPriceInCents));

        await repository.UpdateAsync(part, ct);
        return PartOutput.From(part);
    }
}
