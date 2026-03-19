using System.Text.RegularExpressions;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Domain.Customers;

public sealed partial class Email : ValueObject
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormat();

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var normalized = value.ToLowerInvariant();

        if (!EmailFormat().IsMatch(normalized))
            throw new DomainException($"'{value}' is not a valid email address.");

        Value = normalized;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
