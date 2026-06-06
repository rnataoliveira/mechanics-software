using MechanicsSoftware.Domain.Enums;

namespace MechanicsSoftware.API.Transport.Customers;

public sealed record CreateCustomerRequest(
    string Name,
    string DocumentValue,
    PersonType PersonType,
    string Email,
    string Phone
);
