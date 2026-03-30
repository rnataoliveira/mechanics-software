using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed record ListCustomersQuery(
    string? Name = null,
    string? Document = null
);

public sealed class ListCustomersUseCase(IAppDbContext db)
{
    public async Task<IReadOnlyList<CustomerResponse>> ExecuteAsync(
        ListCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        var customers = db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
            customers = customers.Where(c => c.Name.Contains(query.Name.Trim()));

        if (!string.IsNullOrWhiteSpace(query.Document))
        {
            var normalized = query.Document.Trim();
            customers = customers.Where(c => c.Document.Value == normalized);
        }

        return await customers
            .Select(c => new CustomerResponse(c.Id, c.Name, c.Document.Value, c.Email.Value, c.Phone))
            .ToListAsync(cancellationToken);
    }
}