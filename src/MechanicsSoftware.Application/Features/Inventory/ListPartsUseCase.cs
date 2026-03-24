using MechanicsSoftware.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class ListPartsUseCase(IAppDbContext context)
{
    public async Task<IEnumerable<PartOutput>> ExecuteAsync(
        string? code = null,
        string? name = null,
        CancellationToken ct = default)
    {
        var query = context.Parts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(code))
            query = query.Where(p => p.Code.Contains(code));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        var parts = await query.ToListAsync(ct);
        return parts.Select(PartOutput.From);
    }
}
