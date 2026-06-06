using FluentAssertions;
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

public class UpdateVehicleUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static Vehicle BuildVehicle(Guid? id = null) =>
        Vehicle.Create(id ?? Guid.NewGuid(), new LicensePlate("ABC1234"), "Toyota", "Corolla", 2020, CustomerId);

    private static Mock<IAppDbContext> BuildContext(List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.Vehicles).Returns(MockDbSetHelper.CreateMockDbSet(vehicles ?? []).Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingVehicle_UpdatesAndReturnsResponse()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = BuildVehicle(vehicleId);
        var db = BuildContext([vehicle]);
        var request = new UpdateVehicleCommand(vehicleId, "Honda", "Civic", 2022);

        var result = await new UpdateVehicleHandler(db.Object).ExecuteAsync(request);

        result.Make.Should().Be("Honda");
        result.Model.Should().Be("Civic");
        result.Year.Should().Be(2022);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VehicleNotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();
        var request = new UpdateVehicleCommand(Guid.NewGuid(), "Honda", "Civic", 2022);

        var act = async () => await new UpdateVehicleHandler(db.Object).ExecuteAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
