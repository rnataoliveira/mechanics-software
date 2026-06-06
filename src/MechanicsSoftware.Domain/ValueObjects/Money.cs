using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public int Cents { get; }
    public const string Currency = "BRL";

    public Money(int cents)
    {
        if (cents < 0)
            throw new DomainException("Money amount cannot be negative.");
        Cents = cents;
    }

    public Money Add(Money other) => new(Cents + other.Cents);

    public Money Multiply(int factor)
    {
        if (factor < 0)
            throw new DomainException("Multiply factor cannot be negative.");
        return new(Cents * factor);
    }

    public string ToFormatted()
    {
        var reais = Cents / 100;
        var centavos = Cents % 100;
        return $"R$ {reais},{centavos:D2}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Cents;
    }

    public override string ToString() => ToFormatted();
}
