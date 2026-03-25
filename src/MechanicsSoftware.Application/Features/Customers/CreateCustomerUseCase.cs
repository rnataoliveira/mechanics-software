using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class CreateCustomerUseCase(IAppDbContext db)
{
    public async Task<CustomerOutput> ExecuteAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await db.Customers
            .FirstOrDefaultAsync(c => c.Document.Value == request.Document, cancellationToken);

        if (existing is not null)
            throw new DomainException($"A customer with document '{request.Document}' already exists.");

        var personType = Enum.Parse<PersonType>(request.PersonType, ignoreCase: true);
        var taxId = new TaxId(personType, request.Document);
        var email = new Email(request.Email);

        var customer = Customer.Create(
            Guid.NewGuid(),
            personType,
            taxId,
            request.Name,
            email,
            request.Phone);

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return CustomerOutput.From(customer);
    }
}