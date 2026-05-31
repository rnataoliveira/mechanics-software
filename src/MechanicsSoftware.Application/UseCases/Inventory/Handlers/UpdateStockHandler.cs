using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Inventory.Commands;
using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Inventory.Handlers;

public sealed class UpdateStockHandler(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdateStockCommand command,
                                               CancellationToken cancellationToken = default)
    {
        var part = await context.Parts.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), id);

        part.Replenish(command.Quantity);

        await context.SaveChangesAsync(cancellationToken);
        return PartOutput.From(part);
    }
}
