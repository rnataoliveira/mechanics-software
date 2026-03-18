using System.Text.RegularExpressions;

namespace MechanicsSoftware.Domain.Customers;

public sealed class Email
{
    // Simplified RFC 5322 regex pattern
    private static readonly Regex EmailPattern = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled
    );

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be null or empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailPattern.IsMatch(normalized))
            throw new DomainException($"Invalid email format: {value}");

        Value = normalized;
    }

    public override bool Equals(object? obj)
        => obj is Email other && Value == other.Value;

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString()
        => Value;
}