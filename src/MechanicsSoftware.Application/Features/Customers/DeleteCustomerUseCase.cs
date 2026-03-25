using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class DeleteCustomerUseCase(IAppDbContext context)
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await context.Customers.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Customer), id);

        context.Customers.Remove(customer);
        await context.SaveChangesAsync(ct);
    }
}