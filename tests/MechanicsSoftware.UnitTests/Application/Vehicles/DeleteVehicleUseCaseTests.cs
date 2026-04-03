using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Vehicles;
using MechanicsSoftware.Domain.Vehicles;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

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

        await new DeleteVehicleUseCase(db.Object).ExecuteAsync(vehicleId);

        mockVehicles.Verify(m => m.Remove(vehicle), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_VehicleNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var (db, _) = BuildContext();

        var act = async () => await new DeleteVehicleUseCase(db.Object).ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}
