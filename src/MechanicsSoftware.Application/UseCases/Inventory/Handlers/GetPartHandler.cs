using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Inventory.Handlers;

public sealed class GetPartHandler(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await context.Parts.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), id);

        return PartOutput.From(part);
    }
}
