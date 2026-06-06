using FluentAssertions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Domain.Vehicles;

public class LicensePlateTests
{
    [Theory]
    [InlineData("ABC1D23")]
    [InlineData("XYZ9Z99")]
    public void Constructor_ValidMercosul_Accepted(string plate)
    {
        var lp = new LicensePlate(plate);
        lp.Value.Should().Be(plate.ToUpperInvariant());
    }

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("XYZ9999")]
    public void Constructor_ValidLegacy_Accepted(string plate)
    {
        var lp = new LicensePlate(plate);
        lp.Value.Should().Be(plate.ToUpperInvariant());
    }

    [Theory]
    [InlineData("abc1234")]
    [InlineData("Abc1D23")]
    public void Constructor_LowercaseInput_NormalizedToUppercase(string plate)
    {
        var lp = new LicensePlate(plate);
        lp.Value.Should().Be(plate.ToUpperInvariant());
    }

    [Theory]
    [InlineData("ABC-1234")]
    [InlineData("ABC-1D23")]
    public void Constructor_PlateWithHyphens_HyphensStripped(string plate)
    {
        var lp = new LicensePlate(plate);
        lp.Value.Should().NotContain("-");
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("12A1234")]
    [InlineData("ABCD123")]
    [InlineData("ABC12D3")]
    public void Constructor_InvalidFormat_ThrowsDomainException(string plate)
    {
        var act = () => new LicensePlate(plate);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyOrWhitespace_ThrowsDomainException(string plate)
    {
        var act = () => new LicensePlate(plate);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equality_SamePlate_AreEqual()
    {
        var a = new LicensePlate("ABC1234");
        var b = new LicensePlate("ABC1234");
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentPlates_AreNotEqual()
    {
        var a = new LicensePlate("ABC1234");
        var b = new LicensePlate("XYZ5678");
        (a != b).Should().BeTrue();
    }
}
