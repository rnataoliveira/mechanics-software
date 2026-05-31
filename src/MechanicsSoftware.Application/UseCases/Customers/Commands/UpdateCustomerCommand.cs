namespace MechanicsSoftware.Application.UseCases.Customers.Commands;

public sealed record UpdateCustomerCommand(
    string Name,
    string Email,
    string Phone
);
