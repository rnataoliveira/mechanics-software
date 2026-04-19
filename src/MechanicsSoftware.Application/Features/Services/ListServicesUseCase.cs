using MechanicsSoftware.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Services;

public sealed class ListServicesUseCase(IAppDbContext db)
{
    public async Task<IReadOnlyList<ServiceResponse>> ExecuteAsync(
        ListServicesQuery query, CancellationToken cancellationToken = default)
    {
        var services = db.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
            services = services.Where(s => s.Name.Contains(query.Name.Trim()));

        var result = await services
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return result.Select(ServiceResponse.From).ToList();
    }
}
