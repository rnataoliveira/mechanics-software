using FluentAssertions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Inventory;

public class PartTests
{
    private static readonly Guid ValidId = Guid.NewGuid();
    private static readonly Money ValidPrice = new(1000);
    private static readonly Guid OrderRef = Guid.NewGuid();

    private static Part CreatePart(int initialStock = 10) =>
        Part.Create(ValidId, "ENG-001", "Oil Filter", null, ValidPrice, initialStock);

    [Fact]
    public void Create_ValidArguments_ReturnsPart()
    {
        var part = CreatePart();

        part.Code.Should().Be("ENG-001");
        part.Name.Should().Be("Oil Filter");
        part.StockQuantity.Should().Be(10);
        part.ReservedQuantity.Should().Be(0);
        part.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public void Create_WithInitialStock_AddsInboundMovement()
    {
        var part = CreatePart(5);

        part.Movements.Should().HaveCount(1);
        part.Movements.First().Type.Should().Be(StockMovementType.Inbound);
        part.Movements.First().Quantity.Should().Be(5);
    }

    [Fact]
    public void Create_ZeroInitialStock_NoMovement()
    {
        var part = CreatePart(0);
        part.Movements.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyCode_ThrowsDomainException(string code)
    {
        var act = () => Part.Create(ValidId, code, "Oil Filter", null, ValidPrice);
        act.Should().Throw<DomainException>().WithMessage("*code*");
    }

    [Fact]
    public void Create_NegativeInitialStock_ThrowsDomainException()
    {
        var act = () => Part.Create(ValidId, "ENG-001", "Oil Filter", null, ValidPrice, -1);
        act.Should().Throw<DomainException>().WithMessage("*negative*");
    }

    [Fact]
    public void Create_CodeNormalized_ToUppercase()
    {
        var part = Part.Create(ValidId, "eng-001", "Oil Filter", null, ValidPrice);
        part.Code.Should().Be("ENG-001");
    }

    [Fact]
    public void Reserve_ValidQuantity_IncreasesReserved()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);

        part.ReservedQuantity.Should().Be(3);
        part.AvailableQuantity.Should().Be(7);
        part.StockQuantity.Should().Be(10);
    }

    [Fact]
    public void Reserve_AddsReservationMovement()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);

        var movement = part.Movements.Last();
        movement.Type.Should().Be(StockMovementType.Reservation);
        movement.Quantity.Should().Be(3);
        movement.Reference.Should().Be(OrderRef);
    }

    [Fact]
    public void Reserve_InsufficientAvailable_ThrowsDomainException()
    {
        var part = CreatePart(5);
        var act = () => part.Reserve(6, OrderRef);
        act.Should().Throw<DomainException>().WithMessage("*Insufficient available stock*");
    }

    [Fact]
    public void Reserve_ZeroQuantity_ThrowsDomainException()
    {
        var part = CreatePart(10);
        var act = () => part.Reserve(0, OrderRef);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ConfirmUsage_ValidQuantity_DeductsStock()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        part.ConfirmUsage(3, OrderRef);

        part.StockQuantity.Should().Be(7);
        part.ReservedQuantity.Should().Be(0);
        part.AvailableQuantity.Should().Be(7);
    }

    [Fact]
    public void ConfirmUsage_AddsOutboundMovement()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        part.ConfirmUsage(3, OrderRef);

        var movement = part.Movements.Last();
        movement.Type.Should().Be(StockMovementType.Outbound);
        movement.Quantity.Should().Be(3);
    }

    [Fact]
    public void ConfirmUsage_MoreThanReserved_ThrowsDomainException()
    {
        var part = CreatePart(10);
        part.Reserve(2, OrderRef);
        var act = () => part.ConfirmUsage(3, OrderRef);
        act.Should().Throw<DomainException>().WithMessage("*reserved*");
    }

    [Fact]
    public void ConfirmUsage_WithoutPriorReserve_ThrowsDomainException()
    {
        var part = CreatePart(10);
        var act = () => part.ConfirmUsage(3, OrderRef);
        act.Should().Throw<DomainException>().WithMessage("*reserved*");
    }

    [Fact]
    public void Release_ValidQuantity_RestoresAvailable()
    {
        var part = CreatePart(10);
        part.Reserve(5, OrderRef);
        part.Release(5, OrderRef);

        part.ReservedQuantity.Should().Be(0);
        part.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public void Release_AddsReleaseMovement()
    {
        var part = CreatePart(10);
        part.Reserve(5, OrderRef);
        part.Release(5, OrderRef);

        var movement = part.Movements.Last();
        movement.Type.Should().Be(StockMovementType.Release);
        movement.Quantity.Should().Be(5);
    }

    [Fact]
    public void Release_MoreThanReserved_ThrowsDomainException()
    {
        var part = CreatePart(10);
        part.Reserve(2, OrderRef);
        var act = () => part.Release(3, OrderRef);
        act.Should().Throw<DomainException>().WithMessage("*Cannot release more than reserved*");
    }

    [Fact]
    public void Replenish_ValidQuantity_IncreasesStock()
    {
        var part = CreatePart(10);
        part.Replenish(5);

        part.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void Replenish_AddsInboundMovement()
    {
        var part = CreatePart(0);
        part.Replenish(20);

        var movement = part.Movements.Last();
        movement.Type.Should().Be(StockMovementType.Inbound);
        movement.Quantity.Should().Be(20);
    }

    [Fact]
    public void Replenish_ZeroQuantity_ThrowsDomainException()
    {
        var part = CreatePart(10);
        var act = () => part.Replenish(0);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsDomainException(string name)
    {
        var act = () => Part.Create(ValidId, "ENG-001", name, null, ValidPrice);
        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_NullUnitPrice_ThrowsDomainException()
    {
        var act = () => Part.Create(ValidId, "ENG-001", "Oil Filter", null, null!);
        act.Should().Throw<DomainException>().WithMessage("*price*");
    }

    [Fact]
    public void Reserve_EmptyReference_ThrowsDomainException()
    {
        var part = CreatePart(10);
        var act = () => part.Reserve(3, Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Reference*");
    }

    [Fact]
    public void ConfirmUsage_ZeroQuantity_ThrowsDomainException()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        var act = () => part.ConfirmUsage(0, OrderRef);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ConfirmUsage_EmptyReference_ThrowsDomainException()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        var act = () => part.ConfirmUsage(3, Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Reference*");
    }

    [Fact]
    public void Release_ZeroQuantity_ThrowsDomainException()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        var act = () => part.Release(0, OrderRef);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Release_EmptyReference_ThrowsDomainException()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        var act = () => part.Release(3, Guid.Empty);
        act.Should().Throw<DomainException>().WithMessage("*Reference*");
    }

    [Fact]
    public void Update_ValidArguments_UpdatesProperties()
    {
        var part = CreatePart();
        var newPrice = new Money(2000);

        part.Update("New Name", "New description", newPrice);

        part.Name.Should().Be("New Name");
        part.Description.Should().Be("New description");
        part.UnitPrice.Should().Be(newPrice);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyName_ThrowsDomainException(string name)
    {
        var part = CreatePart();
        var act = () => part.Update(name, null, ValidPrice);
        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void Update_NullUnitPrice_ThrowsDomainException()
    {
        var part = CreatePart();
        var act = () => part.Update("Oil Filter", null, null!);
        act.Should().Throw<DomainException>().WithMessage("*price*");
    }

    [Fact]
    public void HasPendingReservations_WithReservations_ReturnsTrue()
    {
        var part = CreatePart(10);
        part.Reserve(3, OrderRef);
        part.HasPendingReservations().Should().BeTrue();
    }

    [Fact]
    public void HasPendingReservations_WithoutReservations_ReturnsFalse()
    {
        var part = CreatePart(10);
        part.HasPendingReservations().Should().BeFalse();
    }
}
