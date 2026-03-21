using MechanicsSoftware.Domain.Inventory;

namespace MechanicsSoftware.Application.Features.Inventory;

public sealed class ListPartsUseCase(IPartRepository repository)
{
    public async Task<IEnumerable<PartOutput>> ExecuteAsync(
        string? code = null,
        string? name = null,
        CancellationToken ct = default)
    {
        var parts = await repository.ListAsync(code, name, ct);
        return parts.Select(CreatePartUseCase.ToOutput);
    }
}
