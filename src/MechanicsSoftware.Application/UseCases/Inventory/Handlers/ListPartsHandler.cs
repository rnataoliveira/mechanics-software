using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.Inventory.Queries;
using MechanicsSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Inventory.Handlers;

public sealed class ListPartsHandler(IAppDbContext context)
{
    public async Task<IEnumerable<PartOutput>> ExecuteAsync(ListPartsQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<Part> queryParts = context.Parts;

        if (!string.IsNullOrWhiteSpace(query.Code))
            queryParts = queryParts.Where(p => p.Code.Contains(query.Code));

        if (!string.IsNullOrWhiteSpace(query.Name))
            queryParts = queryParts.Where(p => p.Name.Contains(query.Name));

        var parts = await queryParts.ToListAsync(cancellationToken);
        return parts.Select(PartOutput.From);
    }
}
