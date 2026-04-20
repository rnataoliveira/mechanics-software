using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Services;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Services;

public sealed class UpdateServiceUseCase(IAppDbContext db)
{
    public async Task<ServiceResponse> ExecuteAsync(
        Guid id, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await db.Services.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Service), id);

        var nameConflict = await db.Services
            .AnyAsync(s => s.Name == request.Name.Trim() && s.Id != id, cancellationToken);

        if (nameConflict)
            throw new DomainException($"A service named '{request.Name}' already exists.");

        service.Update(
            request.Name,
            request.Description,
            new Money(request.BasePriceInCents),
            request.EstimatedMinutes);

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResponse.From(service);
    }
}
