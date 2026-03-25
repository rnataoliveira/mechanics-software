using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class UpdateCustomerUseCase(IAppDbdb db)
{
    public async Task<CustomerOutput> ExecuteAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        var email = new Email(request.Email);
        customer.Update(request.Name, email, request.Phone);

        await db.SaveChangesAsync(cancellationToken);

        return CustomerOutput.From(customer);
    }
}