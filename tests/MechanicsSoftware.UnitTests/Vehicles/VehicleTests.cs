using FluentAssertions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Domain.Vehicles;

public class VehicleTests
{
    private static readonly Guid ValidId = Guid.NewGuid();
    private static readonly LicensePlate ValidPlate = new("ABC1234");
    private static readonly Guid ValidCustomerId = Guid.NewGuid();
    private static readonly int ValidYear = DateTime.UtcNow.Year;

    [Fact]
    public void Create_ValidArguments_ReturnsVehicle()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);

        vehicle.Id.Should().Be(ValidId);
        vehicle.LicensePlate.Should().Be(ValidPlate);
        vehicle.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Corolla");
        vehicle.Year.Should().Be(ValidYear);
        vehicle.CustomerId.Should().Be(ValidCustomerId);
    }

    [Fact]
    public void Create_NullLicensePlate_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(ValidId, null!, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        act.Should().Throw<DomainException>().WithMessage("*License plate*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyMake_ThrowsDomainException(string make)
    {
        var act = () => Vehicle.Create(ValidId, ValidPlate, make, "Corolla", ValidYear, ValidCustomerId);
        act.Should().Throw<DomainException>().WithMessage("*Make*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyModel_ThrowsDomainException(string model)
    {
        var act = () => Vehicle.Create(ValidId, ValidPlate, "Toyota", model, ValidYear, ValidCustomerId);
        act.Should().Throw<DomainException>().WithMessage("*Model*");
    }

    [Fact]
    public void Create_YearBefore1886_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", 1885, ValidCustomerId);
        act.Should().Throw<DomainException>().WithMessage("*Year*");
    }

    [Fact]
    public void Create_YearAfterCurrentPlusOne_ThrowsDomainException()
    {
        var futureYear = DateTime.UtcNow.Year + 2;
        var act = () => Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", futureYear, ValidCustomerId);
        act.Should().Throw<DomainException>().WithMessage("*Year*");
    }

    [Fact]
    public void Create_YearCurrentPlusOne_Valid()
    {
        var nextYear = DateTime.UtcNow.Year + 1;
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", nextYear, ValidCustomerId);
        vehicle.Year.Should().Be(nextYear);
    }

    [Fact]
    public void Create_Year1886_Valid()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", 1886, ValidCustomerId);
        vehicle.Year.Should().Be(1886);
    }

    [Fact]
    public void Create_EmptyCustomerId_ThrowsDomainException()
    {
        var act = () => Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*CustomerId*");
    }

    [Fact]
    public void Equality_SameId_AreEqual()
    {
        var id = Guid.NewGuid();
        var a = Vehicle.Create(id, new LicensePlate("ABC1234"), "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var b = Vehicle.Create(id, new LicensePlate("XYZ5678"), "Honda", "Civic", ValidYear, ValidCustomerId);

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentIds_AreNotEqual()
    {
        var a = Vehicle.Create(Guid.NewGuid(), new LicensePlate("ABC1234"), "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var b = Vehicle.Create(Guid.NewGuid(), new LicensePlate("ABC1234"), "Toyota", "Corolla", ValidYear, ValidCustomerId);

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Create_MakeWithWhitespace_IsTrimmed()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "  Toyota  ", "Corolla", ValidYear, ValidCustomerId);
        vehicle.Make.Should().Be("Toyota");
    }

    [Fact]
    public void Create_ModelWithWhitespace_IsTrimmed()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "  Corolla  ", ValidYear, ValidCustomerId);
        vehicle.Model.Should().Be("Corolla");
    }

    [Fact]
    public void UpdateLicensePlate_ValidPlate_Updates()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var newPlate = new LicensePlate("XYZ1A23");

        vehicle.UpdateLicensePlate(newPlate);

        vehicle.LicensePlate.Should().Be(newPlate);
    }

    [Fact]
    public void UpdateLicensePlate_NullPlate_ThrowsDomainException()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);

        var act = () => vehicle.UpdateLicensePlate(null!);
        act.Should().Throw<DomainException>().WithMessage("*License plate*");
    }

    [Fact]
    public void Update_ValidArguments_UpdatesProperties()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);

        vehicle.Update("Honda", "Civic", 2020);

        vehicle.Make.Should().Be("Honda");
        vehicle.Model.Should().Be("Civic");
        vehicle.Year.Should().Be(2020);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyMake_ThrowsDomainException(string make)
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var act = () => vehicle.Update(make, "Civic", 2020);
        act.Should().Throw<DomainException>().WithMessage("*Make*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyModel_ThrowsDomainException(string model)
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var act = () => vehicle.Update("Honda", model, 2020);
        act.Should().Throw<DomainException>().WithMessage("*Model*");
    }

    [Fact]
    public void Update_InvalidYear_ThrowsDomainException()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var act = () => vehicle.Update("Honda", "Civic", 1800);
        act.Should().Throw<DomainException>().WithMessage("*Year*");
    }

    [Fact]
    public void Update_WhitespaceMakeAndModel_AreTrimmed()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        vehicle.Update("  Honda  ", "  Civic  ", 2020);
        vehicle.Make.Should().Be("Honda");
        vehicle.Model.Should().Be("Civic");
    }

    [Fact]
    public void Equals_NullObject_ReturnsFalse()
    {
        var vehicle = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        vehicle.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameId_SameHash()
    {
        var a = Vehicle.Create(ValidId, ValidPlate, "Toyota", "Corolla", ValidYear, ValidCustomerId);
        var b = Vehicle.Create(ValidId, new LicensePlate("XYZ1A23"), "Honda", "Civic", 2020, ValidCustomerId);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
