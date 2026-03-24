using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class UpdatePartUseCase(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, UpdatePartInput input,
                                               CancellationToken ct = default)
    {
        var part = await context.Parts.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Part), id);

        part.Update(input.Name, input.Description, new Money(input.UnitPriceInCents));

        await context.SaveChangesAsync(ct);
        return PartOutput.From(part);
    }
}
