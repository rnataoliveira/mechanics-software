using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.Customers.Commands;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MechanicsSoftware.Application.UseCases.Customers.Handlers;

public sealed class CreateCustomerHandler(IAppDbContext db)
{
    public async Task<CustomerResponse> ExecuteAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var documentVo = new TaxId(command.DocumentValue, command.PersonType);

        var customerExisting = await db.Customers
            .AnyAsync(c => c.Document == documentVo, cancellationToken);

        if (customerExisting)
            throw new DomainException($"A customer with document '{command.DocumentValue}' already exists.");

        var customer = Customer.Create(
            id: Guid.NewGuid(),
            personType: command.PersonType,
            taxId: command.DocumentValue,
            name: command.Name,
            email: command.Email,
            phone: command.Phone);

        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);

        return CustomerResponse.From(customer);
    }
}
