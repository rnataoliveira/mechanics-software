using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Features.Customers;

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
