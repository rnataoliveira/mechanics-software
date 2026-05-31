using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.Services.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Services.Handlers;

public sealed class CreateServiceHandler(IAppDbContext db)
{
    public async Task<ServiceResponse> ExecuteAsync(
        CreateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var nameExists = await db.Services
            .AnyAsync(s => s.Name == command.Name.Trim(), cancellationToken);

        if (nameExists)
            throw new DomainException($"A service named '{command.Name}' already exists.");

        var service = Service.Create(
            Guid.NewGuid(),
            command.Name,
            command.Description,
            new Money(command.BasePriceInCents),
            command.EstimatedMinutes);

        db.Services.Add(service);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResponse.From(service);
    }
}
