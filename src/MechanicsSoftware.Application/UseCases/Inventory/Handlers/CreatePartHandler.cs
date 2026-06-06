using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.Inventory.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Inventory.Handlers;

public sealed class CreatePartHandler(IAppDbContext context)
{
    public async Task<PartOutput> ExecuteAsync(CreatePartCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await context.Parts
            .FirstOrDefaultAsync(p => p.Code == command.Code, cancellationToken);

        if (existing is not null)
            throw new DomainException($"A part with code '{command.Code}' already exists.");

        var part = Part.Create(
            Guid.NewGuid(),
            command.Code,
            command.Name,
            command.Description,
            new Money(command.UnitPriceInCents),
            command.InitialStock
        );

        context.Parts.Add(part);
        await context.SaveChangesAsync(cancellationToken);
        return PartOutput.From(part);
    }
}
