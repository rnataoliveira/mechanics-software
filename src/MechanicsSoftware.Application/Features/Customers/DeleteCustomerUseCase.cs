using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class DeleteCustomerUseCase(IAppDbContext db)
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