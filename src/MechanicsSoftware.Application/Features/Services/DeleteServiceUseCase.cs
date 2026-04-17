using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Services;

namespace MechanicsSoftware.Application.Features.Services;

public sealed class DeleteServiceUseCase(IAppDbContext db)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var service = await db.Services.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Service), id);

        db.Services.Remove(service);
        await db.SaveChangesAsync(ct);
    }
}
