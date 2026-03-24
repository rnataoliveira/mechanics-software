using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class CreatePartUseCase(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(CreatePartInput input, CancellationToken ct = default)
    {
        var existing = await context.Parts
            .FirstOrDefaultAsync(p => p.Code == input.Code, ct);

        if (existing is not null)
            throw new DomainException($"A part with code '{input.Code}' already exists.");

        var part = Part.Create(
            Guid.NewGuid(),
            input.Code,
            input.Name,
            input.Description,
            new Money(input.UnitPriceInCents),
            input.InitialStock
        );

        context.Parts.Add(part);
        await context.SaveChangesAsync(ct);
        return PartOutput.From(part);
    }
}
