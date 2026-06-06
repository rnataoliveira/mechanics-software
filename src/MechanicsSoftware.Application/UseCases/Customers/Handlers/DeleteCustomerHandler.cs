using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Customers.Handlers;

public sealed class DeleteCustomerHandler(IAppDbContext db)
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        db.Customers.Remove(customer);
        await db.SaveChangesAsync(cancellationToken);
    }
}
