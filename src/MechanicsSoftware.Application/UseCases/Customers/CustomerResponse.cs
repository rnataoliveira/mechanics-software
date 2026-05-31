using MechanicsSoftware.Domain.Entities;

namespace MechanicsSoftware.Application.UseCases.Customers;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string DocumentValue,
    string Email,
    string Phone
)
{
    public static CustomerResponse From(Customer c) =>
        new(c.Id, c.Name, c.Document.Value, c.Email.Value, c.Phone);
}
