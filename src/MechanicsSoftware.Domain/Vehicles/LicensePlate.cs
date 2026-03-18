using System.Text.RegularExpressions;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Vehicles;

public sealed partial class LicensePlate : ValueObject
{
    [GeneratedRegex(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$")]
    private static partial Regex MercosulFormat();

    [GeneratedRegex(@"^[A-Z]{3}[0-9]{4}$")]
    private static partial Regex LegacyFormat();

    public string Value { get; }

    public LicensePlate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("License plate cannot be empty.");

        var normalised = value.ToUpperInvariant().Replace("-", "");

        if (!MercosulFormat().IsMatch(normalised) && !LegacyFormat().IsMatch(normalised))
            throw new DomainException($"'{value}' is not a valid Brazilian license plate.");

        Value = normalised;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
