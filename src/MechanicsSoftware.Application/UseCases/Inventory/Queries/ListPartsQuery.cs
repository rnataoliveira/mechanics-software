namespace MechanicsSoftware.Application.UseCases.Inventory.Queries;

public sealed record ListPartsQuery(string? Code = null, string? Name = null);
