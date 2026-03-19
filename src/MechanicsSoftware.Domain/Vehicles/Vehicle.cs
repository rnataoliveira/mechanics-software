using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Vehicles;

public sealed class Vehicle : Entity<Guid>
{
    public LicensePlate LicensePlate { get; private set; } = null!;
    public string Make { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int Year { get; private set; }
    public Guid CustomerId { get; private set; }

    private Vehicle() { }

    public static Vehicle Create(
        Guid id,
        LicensePlate licensePlate,
        string make,
        string model,
        int year,
        Guid customerId)
    {
        if (licensePlate is null)
            throw new DomainException("License plate is required.");

        if (string.IsNullOrWhiteSpace(make))
            throw new DomainException("Make is required.");

        if (string.IsNullOrWhiteSpace(model))
            throw new DomainException("Model is required.");

        make = make.Trim();
        model = model.Trim();

        var currentYear = DateTime.UtcNow.Year;
        if (year < 1886 || year > currentYear + 1)
            throw new DomainException($"Year must be between 1886 and {currentYear + 1}.");

        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");

        return new Vehicle
        {
            Id = id,
            LicensePlate = licensePlate,
            Make = make,
            Model = model,
            Year = year,
            CustomerId = customerId
        };
    }

    public void UpdateLicensePlate(LicensePlate newLicensePlate)
    {
        if (newLicensePlate is null)
            throw new DomainException("License plate is required.");

        LicensePlate = newLicensePlate;
    }
}

