using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Services;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Services;

public sealed class CreateServiceUseCase(IAppDbContext db)
{
    public async Task<ServiceResponse> ExecuteAsync(
        CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var nameExists = await db.Services
            .AnyAsync(s => s.Name == request.Name.Trim(), cancellationToken);

        if (nameExists)
            throw new DomainException($"A service named '{request.Name}' already exists.");

        var service = Service.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            new Money(request.BasePriceInCents),
            request.EstimatedMinutes);

        db.Services.Add(service);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResponse.From(service);
    }
}
