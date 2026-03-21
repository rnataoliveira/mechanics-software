using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class GetPartUseCase(IPartRepository repository)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct)
                   ?? throw new DomainException($"Part with id '{id}' not found.");

        return CreatePartUseCase.ToOutput(part);
    }
}
