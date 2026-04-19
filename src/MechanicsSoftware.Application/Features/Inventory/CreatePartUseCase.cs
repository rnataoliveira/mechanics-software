using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class CreatePartUseCase(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(CreatePartRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await context.Parts
            .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

        if (existing is not null)
            throw new DomainException($"A part with code '{request.Code}' already exists.");

        var part = Part.Create(
            Guid.NewGuid(),
            request.Code,
            request.Name,
            request.Description,
            new Money(request.UnitPriceInCents),
            request.InitialStock
        );

        context.Parts.Add(part);
        await context.SaveChangesAsync(cancellationToken);
        return PartOutput.From(part);
    }
}
