using MechanicsSoftware.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class ListCustomersUseCase(IAppDbContext db)
{
    public async Task<ListCustomersOutput> ExecuteAsync(ListCustomersRequest request, CancellationToken cancellationToken = default)
    {
        var query = db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(c => c.Name.Contains(request.Name));

        if (!string.IsNullOrWhiteSpace(request.Document))
            query = query.Where(c => c.Document.Value == request.Document);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new ListCustomersOutput(
            items.Select(CustomerOutput.From).ToList(),
            totalCount,
            request.Page,
            request.PageSize);
    }
}