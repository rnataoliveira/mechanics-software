using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class UpdateStockUseCase(IPartRepository repository)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdateStockInput input,
                                               CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct)
                   ?? throw new DomainException($"Part with id '{id}' not found.");

        // Delega ao domínio — não manipula estoque diretamente
        part.Replenish(input.Quantity);

        await repository.UpdateAsync(part, ct);
        return CreatePartUseCase.ToOutput(part);
    }
}
