namespace MechanicsSoftware.API.Transport.Inventory;

public sealed record UpdatePartRequest(string Name, string? Description, int UnitPriceInCents);
