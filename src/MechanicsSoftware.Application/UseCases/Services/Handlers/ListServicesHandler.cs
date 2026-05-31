using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.Services.Queries;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Services.Handlers;

public sealed class ListServicesHandler(IAppDbContext db)
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
