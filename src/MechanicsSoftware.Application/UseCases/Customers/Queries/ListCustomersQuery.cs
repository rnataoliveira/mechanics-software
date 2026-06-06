namespace MechanicsSoftware.Application.UseCases.Customers.Queries;

public sealed record ListCustomersQuery(
    string? Name = null,
    string? Document = null
);
