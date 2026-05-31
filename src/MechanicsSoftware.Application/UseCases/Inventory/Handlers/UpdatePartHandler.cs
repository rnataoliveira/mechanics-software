using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Inventory.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;

namespace MechanicsSoftware.Application.UseCases.Inventory.Handlers;

public sealed class UpdatePartHandler(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdatePartCommand command,
                                               CancellationToken cancellationToken = default)
    {
        var part = await context.Parts.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), id);

        part.Update(command.Name, command.Description, new Money(command.UnitPriceInCents));

        await context.SaveChangesAsync(cancellationToken);
        return PartOutput.From(part);
    }
}
