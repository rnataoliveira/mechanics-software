using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class GetCustomerUseCase(IAppDbContext db)
{
    public async Task<CustomerResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        return ToResponse(customer);
    }

    private static CustomerResponse ToResponse(Customer c) =>
        new(c.Id, c.Name, c.Document.Value, c.Email.Value, c.Phone);
}