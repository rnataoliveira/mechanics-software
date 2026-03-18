using FluentAssertions;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Shared;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidCents_SetsValue()
    {
        var money = new Money(15000);
        money.Cents.Should().Be(15000);
    }

    [Fact]
    public void Constructor_WithZero_IsAllowed()
    {
        var money = new Money(0);
        money.Cents.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNegativeCents_ThrowsDomainException()
    {
        var act = () => new Money(-1);
        act.Should().Throw<DomainException>()
            .WithMessage("*negative*");
    }

    [Fact]
    public void Add_ReturnsSumOfCents()
    {
        var a = new Money(10000);
        var b = new Money(5000);
        a.Add(b).Cents.Should().Be(15000);
    }

    [Fact]
    public void Multiply_ReturnsScaledCents()
    {
        var money = new Money(10000);
        money.Multiply(3).Cents.Should().Be(30000);
    }

    [Fact]
    public void Multiply_ByZero_ReturnsZero()
    {
        var money = new Money(10000);
        money.Multiply(0).Cents.Should().Be(0);
    }

    [Fact]
    public void Multiply_ByNegativeFactor_ThrowsDomainException()
    {
        var money = new Money(10000);
        var act = () => money.Multiply(-1);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(15000, "R$ 150,00")]
    [InlineData(100, "R$ 1,00")]
    [InlineData(5, "R$ 0,05")]
    [InlineData(0, "R$ 0,00")]
    [InlineData(99999, "R$ 999,99")]
    public void ToFormatted_ReturnsCorrectString(int cents, string expected)
    {
        new Money(cents).ToFormatted().Should().Be(expected);
    }

    [Fact]
    public void Equality_SameCents_AreEqual()
    {
        var a = new Money(500);
        var b = new Money(500);
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCents_AreNotEqual()
    {
        var a = new Money(500);
        var b = new Money(600);
        (a != b).Should().BeTrue();
    }
}
