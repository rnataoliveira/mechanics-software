using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Services;

public sealed class Service : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Money BasePrice { get; private set; } = null!;
    public int EstimatedMinutes { get; private set; }

    private Service() { }

    public static Service Create(
        Guid id,
        string name,
        string? description,
        Money basePrice,
        int estimatedMinutes = 0)
    {
        if (id == Guid.Empty)
            throw new DomainException("Id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        if (basePrice is null)
            throw new DomainException("Base price is required.");

        if (estimatedMinutes <= 0)
            throw new DomainException("EstimatedMinutes must be positive.");

        return new Service
        {
            Id = id,
            Name = name.Trim(),
            Description = description?.Trim(),
            BasePrice = basePrice,
            EstimatedMinutes = estimatedMinutes
        };
    }

    public void Update(string name, string? description, Money basePrice, int estimatedMinutes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        if (basePrice is null)
            throw new DomainException("Base price is required.");

        if (estimatedMinutes <= 0)
            throw new DomainException("EstimatedMinutes must be positive.");

        Name = name.Trim();
        Description = description?.Trim();
        BasePrice = basePrice;
        EstimatedMinutes = estimatedMinutes;
    }
}

