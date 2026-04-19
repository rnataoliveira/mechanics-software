using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Services;

namespace MechanicsSoftware.Application.Features.Services;

public sealed class GetServiceUseCase(IAppDbContext db)
{
    public async Task<ServiceResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await db.Services.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Service), id);

        return ServiceResponse.From(service);
    }
}
