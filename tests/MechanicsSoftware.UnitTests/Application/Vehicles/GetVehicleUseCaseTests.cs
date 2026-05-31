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

public class GetVehicleUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static Vehicle BuildVehicle(Guid? id = null) =>
        Vehicle.Create(id ?? Guid.NewGuid(), new LicensePlate("ABC1234"), "Toyota", "Corolla", 2020, CustomerId);

    private static Mock<IAppDbContext> BuildContext(List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.Vehicles).Returns(MockDbSetHelper.CreateMockDbSet(vehicles ?? []).Object);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingVehicle_ReturnsVehicleResponse()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = BuildVehicle(vehicleId);
        var db = BuildContext([vehicle]);

        var result = await new GetVehicleHandler(db.Object).ExecuteAsync(vehicleId);

        result.Id.Should().Be(vehicleId);
        result.LicensePlate.Should().Be("ABC1234");
        result.Make.Should().Be("Toyota");
        result.Model.Should().Be("Corolla");
        result.CustomerId.Should().Be(CustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_VehicleNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var db = BuildContext();

        var act = async () => await new GetVehicleHandler(db.Object).ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}
