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

public class DeleteVehicleUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();

    private static Vehicle BuildVehicle(Guid? id = null) =>
        Vehicle.Create(id ?? Guid.NewGuid(), new LicensePlate("ABC1234"), "Toyota", "Corolla", 2020, CustomerId);

    private static (Mock<IAppDbContext> db, Mock<DbSet<Vehicle>> vehicles)
        BuildContext(List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockVehicles = MockDbSetHelper.CreateMockDbSet(vehicles ?? []);

        db.Setup(d => d.Vehicles).Returns(mockVehicles.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (db, mockVehicles);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingVehicle_RemovesAndSaves()
    {
        var vehicleId = Guid.NewGuid();
        var vehicle = BuildVehicle(vehicleId);
        var (db, mockVehicles) = BuildContext([vehicle]);

        await new DeleteVehicleHandler(db.Object).ExecuteAsync(vehicleId);

        mockVehicles.Verify(m => m.Remove(vehicle), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VehicleNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var (db, _) = BuildContext();

        var act = async () => await new DeleteVehicleHandler(db.Object).ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}
