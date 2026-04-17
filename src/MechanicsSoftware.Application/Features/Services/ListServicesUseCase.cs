using MechanicsSoftware.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Services;

public sealed class ListServicesUseCase(IAppDbContext db)
{
    public async Task<IReadOnlyList<ServiceResponse>> ExecuteAsync(
        ListServicesQuery query, CancellationToken ct = default)
    {
        var services = db.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
            services = services.Where(s => s.Name.Contains(query.Name.Trim()));

        return await services
            .OrderBy(s => s.Name)
            .Select(s => new ServiceResponse(
                s.Id, s.Name, s.Description,
                s.BasePrice.Cents, s.BasePrice.ToFormatted(),
                s.EstimatedMinutes))
            .ToListAsync(ct);
    }
}
