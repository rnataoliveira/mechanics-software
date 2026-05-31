using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Services.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Services.Handlers;

public sealed class UpdateServiceHandler(IAppDbContext db)
{
    public async Task<ServiceResponse> ExecuteAsync(
        Guid id, UpdateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await db.Services.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Service), id);

        var nameConflict = await db.Services
            .AnyAsync(s => s.Name == command.Name.Trim() && s.Id != id, cancellationToken);

        if (nameConflict)
            throw new DomainException($"A service named '{command.Name}' already exists.");

        service.Update(
            command.Name,
            command.Description,
            new Money(command.BasePriceInCents),
            command.EstimatedMinutes);

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResponse.From(service);
    }
}
