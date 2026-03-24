using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Inventory;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class UpdateStockUseCase(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdateStockInput input,
                                               CancellationToken ct = default)
    {
        var part = await context.Parts.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Part), id);

        part.Replenish(input.Quantity);

        await context.SaveChangesAsync(ct);
        return PartOutput.From(part);
    }
}
