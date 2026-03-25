using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class UpdateCustomerUseCase(IAppDbContext context)
{
    public async Task<CustomerOutput> ExecuteAsync(UpdateCustomerInput input, CancellationToken ct = default)
    {
        var customer = await context.Customers.FindAsync([input.Id], ct)
            ?? throw new NotFoundException(nameof(Customer), input.Id);

        var email = new Email(input.Email);
        customer.Update(input.Name, email, input.Phone);

        await context.SaveChangesAsync(ct);

        return CustomerOutput.From(customer);
    }
}