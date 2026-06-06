using MechanicsSoftware.Domain.Enums;

namespace MechanicsSoftware.Application.UseCases.Customers.Commands;

public sealed record CreateCustomerCommand(
    string Name,
    string DocumentValue,
    PersonType PersonType,
    string Email,
    string Phone
);
