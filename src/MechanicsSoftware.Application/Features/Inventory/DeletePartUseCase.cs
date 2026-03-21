using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class DeletePartUseCase(IPartRepository repository)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var part = await repository.GetByIdAsync(id, ct)
                   ?? throw new DomainException($"Part with id '{id}' not found.");

        if (part.HasPendingReservations())
            throw new DomainException(
                "Cannot delete a part with pending reservations in service orders.");

        await repository.DeleteAsync(part, ct);
    }
}
