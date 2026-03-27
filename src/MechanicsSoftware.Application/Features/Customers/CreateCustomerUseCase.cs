using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string DocumentValue,
    string PersonType,
    string Email,
    string Phone
);

public sealed class CreateCustomerUseCase(IAppDbContext db)
{
    public async Task<CustomerResponse> ExecuteAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customerExisting = await db.Customers
            .AnyAsync(c => c.Document.Value == request.DocumentValue, cancellationToken);

        if (!customerExisting)
            throw new DomainException($"A customer with document '{request.DocumentValue}' already exists.");

        var personType = Enum.Parse<PersonType>(request.PersonType, ignoreCase: true);
        var taxId = new TaxId(personType: personType, input: request.DocumentValue);
        var email = new Email(request.Email);

        var customer = Customer.Create(
            id: Guid.NewGuid(),
            personType: personType,
            taxId: request.DocumentValue,
            name: request.Name,
            email: request.Email,
            phone: request.Phone);

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
    }
    private static CustomerResponse ToResponse(Customer c) =>
        new(c.Id, c.Name, c.Document.Value, c.Email.Value, c.Phone);
}