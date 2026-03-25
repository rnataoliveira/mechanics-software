using MechanicsSoftware.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class ListCustomersUseCase(IAppDbContext context)
{
    public async Task<ListCustomersOutput> ExecuteAsync(ListCustomersInput input, CancellationToken ct = default)
    {
        var query = context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.Name))
            query = query.Where(c => c.Name.Contains(input.Name));

        if (!string.IsNullOrWhiteSpace(input.Document))
            query = query.Where(c => c.Document.Value == input.Document);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .ToListAsync(ct);

        return new ListCustomersOutput(
            items.Select(CustomerOutput.From).ToList(),
            totalCount,
            input.Page,
            input.PageSize);
    }
}