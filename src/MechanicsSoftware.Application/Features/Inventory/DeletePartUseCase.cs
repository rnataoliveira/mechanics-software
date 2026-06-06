using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class DeletePartUseCase(IAppDbContext context)
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
