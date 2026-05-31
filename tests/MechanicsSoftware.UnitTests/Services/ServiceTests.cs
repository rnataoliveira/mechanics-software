using FluentAssertions;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Services;

public class ServiceTests
{
    private static readonly Guid ValidId = Guid.NewGuid();
    private static readonly Money ValidPrice = new(50000); // R$ 500.00

    [Fact]
    public void Create_ValidService_Succeeds()
    {
        var service = Service.Create(
            ValidId,
            "Oil Change",
            "Standard oil and filter replacement",
            ValidPrice,
            30);

        service.Id.Should().Be(ValidId);
        service.Name.Should().Be("Oil Change");
        service.Description.Should().Be("Standard oil and filter replacement");
        service.BasePrice.Should().Be(ValidPrice);
        service.EstimatedMinutes.Should().Be(30);
    }

    [Fact]
    public void Create_ValidServiceNoDescription_Succeeds()
    {
        var service = Service.Create(
            ValidId,
            "Oil Change",
            null,
            ValidPrice,
            30);

        service.Description.Should().BeNull();
    }

    [Fact]
    public void Create_EmptyId_ThrowsDomainException()
    {
        var act = () => Service.Create(
            Guid.Empty,
            "Oil Change",
            "Standard oil and filter replacement",
            ValidPrice,
            30);

        act.Should().Throw<DomainException>()
            .WithMessage("*Id is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyOrWhitespaceName_ThrowsDomainException(string? name)
    {
        var act = () => Service.Create(
            ValidId,
            name ?? "",
            "Description",
            ValidPrice,
            30);

        act.Should().Throw<DomainException>()
            .WithMessage("*Name is required*");
    }

    [Fact]
    public void Create_NullBasePrice_ThrowsDomainException()
    {
        var act = () => Service.Create(
            ValidId,
            "Oil Change",
            "Description",
            null!,
            30);

        act.Should().Throw<DomainException>()
            .WithMessage("*Base price is required*");
    }

    [Fact]
    public void Create_NegativeBasePrice_ThrowsDomainException()
    {
        var act = () => Service.Create(
            ValidId,
            "Oil Change",
            "Description",
            new Money(-1),
            30);

        act.Should().Throw<DomainException>()
            .WithMessage("*Money amount cannot be negative*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_NonPositiveEstimatedMinutes_ThrowsDomainException(int minutes)
    {
        var act = () => Service.Create(
            ValidId,
            "Oil Change",
            "Description",
            ValidPrice,
            minutes);

        act.Should().Throw<DomainException>()
            .WithMessage("*EstimatedMinutes must be positive*");
    }

    [Fact]
    public void Create_NameWhitespaceNormalized()
    {
        var service = Service.Create(
            ValidId,
            "  Oil Change  ",
            "Description",
            ValidPrice,
            30);

        service.Name.Should().Be("Oil Change");
    }

    [Fact]
    public void Create_DescriptionWhitespaceNormalized()
    {
        var service = Service.Create(
            ValidId,
            "Oil Change",
            "  Standard oil and filter replacement  ",
            ValidPrice,
            30);

        service.Description.Should().Be("Standard oil and filter replacement");
    }

    [Fact]
    public void Create_PriceSnapshotted()
    {
        var price = new Money(50000);
        var service = Service.Create(
            ValidId,
            "Oil Change",
            "Description",
            price,
            30);

        service.BasePrice.Should().Be(price);
    }

    [Fact]
    public void Equality_SameId_AreEqual()
    {
        var id = Guid.NewGuid();
        var service1 = Service.Create(id, "Oil Change", "Description", ValidPrice, 30);
        var service2 = Service.Create(id, "Tire Rotation", "Different description", new Money(30000), 45);

        service1.Should().Be(service2);
    }

    [Fact]
    public void Equality_DifferentId_AreNotEqual()
    {
        var service1 = Service.Create(Guid.NewGuid(), "Oil Change", "Description", ValidPrice, 30);
        var service2 = Service.Create(Guid.NewGuid(), "Oil Change", "Description", ValidPrice, 30);

        service1.Should().NotBe(service2);
    }
}

