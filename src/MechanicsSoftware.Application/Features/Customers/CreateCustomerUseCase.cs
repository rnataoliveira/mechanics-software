using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.Features.Customers;

public sealed class CreateCustomerUseCase(IAppDbContext context)
{
    public async Task<CustomerOutput> ExecuteAsync(CreateCustomerInput input, CancellationToken ct = default)
    {
        var existing = await context.Customers
            .FirstOrDefaultAsync(c => c.Document.Value == input.Document, ct);

        if (existing is not null)
            throw new DomainException($"A customer with document '{input.Document}' already exists.");

        var personType = Enum.Parse<PersonType>(input.PersonType, ignoreCase: true);
        var taxId = new TaxId(personType, input.Document);
        var email = new Email(input.Email);

        var customer = Customer.Create(
            Guid.NewGuid(),
            personType,
            taxId,
            input.Name,
            email,
            input.Phone);

        context.Customers.Add(customer);
        await context.SaveChangesAsync(ct);

        return CustomerOutput.From(customer);
    }
}