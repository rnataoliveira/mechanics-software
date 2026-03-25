using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class GetCustomerUseCase(IAppDbContext db)
{
    public async Task<CustomerOutput> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers.FindAsync([id], cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        return CustomerOutput.From(customer);
    }
}