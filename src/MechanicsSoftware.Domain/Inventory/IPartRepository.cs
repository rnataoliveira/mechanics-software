namespace MechanicsSoftware.Domain.Inventory;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Part?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IEnumerable<Part>> ListAsync(string? code, string? name, CancellationToken ct = default);
    Task AddAsync(Part part, CancellationToken ct = default);
    Task UpdateAsync(Part part, CancellationToken ct = default);
    Task DeleteAsync(Part part, CancellationToken ct = default);
}
