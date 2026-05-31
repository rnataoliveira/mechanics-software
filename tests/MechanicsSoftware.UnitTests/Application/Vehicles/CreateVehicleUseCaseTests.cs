using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Vehicles;
using MechanicsSoftware.Application.UseCases.Vehicles.Commands;
using MechanicsSoftware.Application.UseCases.Vehicles.Handlers;
using MechanicsSoftware.Application.UseCases.Vehicles.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Vehicles;

public class CreateVehicleUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private const string ValidName  = "Joel Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidEmail = "joel@email.com";
    private const string ValidPhone = "11999999999";
    private const string ValidPlate = "ABC1234";
    private const string ValidMake  = "Toyota";
    private const string ValidModel = "Corolla";
    private const int    ValidYear  = 2020;

    private static Customer BuildCustomer(Guid? id = null) =>
        Customer.Create(id ?? CustomerId, ValidName, ValidCpf, PersonType.INDIVIDUAL, ValidEmail, ValidPhone);

    private static (Mock<IAppDbContext> db, Mock<DbSet<Customer>> customers, Mock<DbSet<Vehicle>> vehicles)
        BuildContext(List<Customer>? customers = null, List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockCustomers = MockDbSetHelper.CreateMockDbSet(customers ?? []);
        var mockVehicles = MockDbSetHelper.CreateMockDbSet(vehicles ?? []);

        db.Setup(d => d.Customers).Returns(mockCustomers.Object);
        db.Setup(d => d.Vehicles).Returns(mockVehicles.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (db, mockCustomers, mockVehicles);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsVehicleResponse()
    {
        var customer = BuildCustomer();
        var (db, _, mockVehicles) = BuildContext(customers: [customer]);

        var useCase = new CreateVehicleHandler(db.Object);
        var request = new CreateVehicleCommand(ValidPlate, ValidMake, ValidModel, ValidYear, CustomerId);

        var result = await useCase.ExecuteAsync(request);

        result.LicensePlate.Should().Be(ValidPlate);
        result.Make.Should().Be(ValidMake);
        result.Model.Should().Be(ValidModel);
        result.Year.Should().Be(ValidYear);
        result.CustomerId.Should().Be(CustomerId);
        mockVehicles.Verify(m => m.Add(It.IsAny<Vehicle>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        var (db, _, _) = BuildContext();

        var useCase = new CreateVehicleHandler(db.Object);
        var request = new CreateVehicleCommand(ValidPlate, ValidMake, ValidModel, ValidYear, CustomerId);

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{CustomerId}*");
    }

    [Fact]
    public async Task ExecuteAsync_DuplicatePlate_ThrowsDomainException()
    {
        var customer = BuildCustomer();
        var existingVehicle = Vehicle.Create(Guid.NewGuid(), new LicensePlate(ValidPlate), "Honda", "Civic", 2019, CustomerId);
        var (db, _, _) = BuildContext(customers: [customer], vehicles: [existingVehicle]);

        var useCase = new CreateVehicleHandler(db.Object);
        var request = new CreateVehicleCommand(ValidPlate, ValidMake, ValidModel, ValidYear, CustomerId);

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<DomainException>().WithMessage($"*{ValidPlate}*");
    }

    [Fact]
    public async Task ExecuteAsync_MercosulPlate_NormalizesAndCreates()
    {
        var customer = BuildCustomer();
        var (db, _, _) = BuildContext(customers: [customer]);

        var useCase = new CreateVehicleHandler(db.Object);
        var request = new CreateVehicleCommand("abc1A23", ValidMake, ValidModel, ValidYear, CustomerId);

        var result = await useCase.ExecuteAsync(request);

        result.LicensePlate.Should().Be("ABC1A23");
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPlate_ThrowsDomainException()
    {
        var customer = BuildCustomer();
        var (db, _, _) = BuildContext(customers: [customer]);

        var useCase = new CreateVehicleHandler(db.Object);
        var request = new CreateVehicleCommand("INVALID", ValidMake, ValidModel, ValidYear, CustomerId);

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<DomainException>();
    }
}
