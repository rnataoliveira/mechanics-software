using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Inventory;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class GetPartUseCase(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await context.Parts.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Part), id);

        return PartOutput.From(part);
    }
}
