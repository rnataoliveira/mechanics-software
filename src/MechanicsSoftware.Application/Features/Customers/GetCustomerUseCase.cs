using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class GetCustomerUseCase(IAppDbContext context)
{
    public async Task<CustomerOutput> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await context.Customers.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Customer), id);

        return CustomerOutput.From(customer);
    }
}