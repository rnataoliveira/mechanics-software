using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Vehicles;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.Domain.Vehicles;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Vehicles;

public class CreateVehicleUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static Customer BuildCustomer(Guid? id = null) =>
        Customer.Create(id ?? CustomerId, "Joel Silva", "52998224725", PersonType.INDIVIDUAL, "joel@email.com", "11999999999");

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

        var useCase = new CreateVehicleUseCase(db.Object);
        var request = new CreateVehicleRequest("ABC1234", "Toyota", "Corolla", 2020, CustomerId);

        var result = await useCase.ExecuteAsync(request);

        result.LicensePlate.Should().Be("ABC1234");
        result.Make.Should().Be("Toyota");
        result.Model.Should().Be("Corolla");
        result.Year.Should().Be(2020);
        result.CustomerId.Should().Be(CustomerId);
        mockVehicles.Verify(m => m.Add(It.IsAny<Vehicle>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        var (db, _, _) = BuildContext();

        var useCase = new CreateVehicleUseCase(db.Object);
        var request = new CreateVehicleRequest("ABC1234", "Toyota", "Corolla", 2020, CustomerId);

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{CustomerId}*");
    }

    [Fact]
    public async Task ExecuteAsync_DuplicatePlate_ThrowsDomainException()
    {
        var customer = BuildCustomer();
        var existingVehicle = Vehicle.Create(Guid.NewGuid(), new LicensePlate("ABC1234"), "Honda", "Civic", 2019, CustomerId);
        var (db, _, _) = BuildContext(customers: [customer], vehicles: [existingVehicle]);

        var useCase = new CreateVehicleUseCase(db.Object);
        var request = new CreateVehicleRequest("ABC1234", "Toyota", "Corolla", 2020, CustomerId);

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*ABC1234*");
    }

    [Fact]
    public async Task ExecuteAsync_MercosulPlate_NormalizesAndCreates()
    {
        var customer = BuildCustomer();
        var (db, _, _) = BuildContext(customers: [customer]);

        var useCase = new CreateVehicleUseCase(db.Object);
        var request = new CreateVehicleRequest("abc1A23", "Toyota", "Corolla", 2020, CustomerId);

        var result = await useCase.ExecuteAsync(request);

        result.LicensePlate.Should().Be("ABC1A23");
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPlate_ThrowsDomainException()
    {
        var customer = BuildCustomer();
        var (db, _, _) = BuildContext(customers: [customer]);

        var useCase = new CreateVehicleUseCase(db.Object);
        var request = new CreateVehicleRequest("INVALID", "Toyota", "Corolla", 2020, CustomerId);

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<DomainException>();
    }
}
