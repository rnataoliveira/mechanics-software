using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class UpdateStockUseCase(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdateStockRequest request,
                                               CancellationToken cancellationToken = default)
    {
        var part = await context.Parts.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), id);

        part.Replenish(request.Quantity);

        await context.SaveChangesAsync(cancellationToken);
        return PartOutput.From(part);
    }
}
