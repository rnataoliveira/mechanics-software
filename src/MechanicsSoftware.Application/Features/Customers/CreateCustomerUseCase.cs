using MechanicsSoftware.Application.Common;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string DocumentValue,
    PersonType PersonType,
    string Email,
    string Phone
);

public sealed class CreateCustomerUseCase(IAppDbContext db)
{
    public async Task<CustomerResponse> ExecuteAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var documentVo = new TaxId(request.DocumentValue, request.PersonType);

        var customerExisting = await db.Customers
            .AnyAsync(c => c.Document == documentVo, cancellationToken);

        if (customerExisting)
            throw new DomainException($"A customer with document '{request.DocumentValue}' already exists.");

        var customer = Customer.Create(
            id: Guid.NewGuid(),
            personType: request.PersonType,
            taxId: request.DocumentValue,
            name: request.Name,
            email: request.Email,
            phone: request.Phone);

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return CustomerResponse.From(customer);
    }
}