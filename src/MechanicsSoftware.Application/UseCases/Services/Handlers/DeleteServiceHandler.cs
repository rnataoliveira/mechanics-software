using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Services.Handlers;

public sealed class DeleteServiceHandler(IAppDbContext db)
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await db.Services.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Service), id);

        db.Services.Remove(service);
        await db.SaveChangesAsync(cancellationToken);
    }
}
