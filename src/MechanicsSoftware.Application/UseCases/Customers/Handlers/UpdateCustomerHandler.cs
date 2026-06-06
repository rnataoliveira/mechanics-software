using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Customers.Commands;
using MechanicsSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Customers.Handlers;

public sealed class UpdateCustomerHandler(IAppDbContext db)
{
    public async Task<CustomerResponse> ExecuteAsync(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        customer.Update(
            name: command.Name,
            email: command.Email,
            phone: command.Phone);

        await db.SaveChangesAsync(cancellationToken);

        return CustomerResponse.From(customer);
    }
}
