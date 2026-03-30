namespace MechanicsSoftware.Application.Features.Customers;

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string DocumentValue,
    string Email,
    string Phone
);
