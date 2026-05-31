using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed record UpdateCustomerRequest(
    string Name,
    string Email,
    string Phone
);

public sealed class UpdateCustomerUseCase(IAppDbContext db)
{
    public async Task<CustomerResponse> ExecuteAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        customer.Update(
            name: request.Name,
            email: request.Email,
            phone: request.Phone);

        await db.SaveChangesAsync(cancellationToken);

        return CustomerResponse.From(customer);
    }
}