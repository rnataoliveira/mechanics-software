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
            var digits = string.Concat(query.Document.Where(char.IsDigit));
            var pt = digits.Length == 14 ? PersonType.COMPANY : PersonType.INDIVIDUAL;
            var documentVo = new TaxId(digits, pt);
            customers = customers.Where(c => c.Document == documentVo);
        }

        return await customers
            .Select(c => CustomerResponse.From(c))
            .ToListAsync(cancellationToken);
    }
}