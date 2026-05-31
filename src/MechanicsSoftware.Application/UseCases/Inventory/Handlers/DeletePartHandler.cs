using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.UseCases.Inventory.Handlers;

public sealed class DeletePartHandler(IAppDbContext context)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var part = await context.Parts.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Part), id);

        if (part.HasPendingReservations())
            throw new DomainException(
                "Cannot delete a part with pending reservations in service orders.");

        context.Parts.Remove(part);
        await context.SaveChangesAsync(ct);
    }
}
