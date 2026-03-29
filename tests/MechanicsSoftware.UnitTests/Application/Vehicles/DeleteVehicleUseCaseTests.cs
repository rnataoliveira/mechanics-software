using FluentAssertions;
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
    private static readonly LicensePlate ValidPlate = new("ABC1234");

    private static Vehicle BuildVehicle(Guid? id = null) =>
        Vehicle.Create(id ?? Guid.NewGuid(), ValidPlate, "Toyota", "Corolla", 2020, CustomerId);

    private static (Mock<IAppDbContext> db, Mock<Microsoft.EntityFrameworkCore.DbSet<Vehicle>> mockVehicles)
        BuildContext(List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockVehicles = MockDbSetHelper.CreateMockDbSet(vehicles ?? []);
        db.Setup(d => d.Vehicles).Returns(mockVehicles.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (db, mockVehicles);
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_RemovesVehicle()
    {
        var id = Guid.NewGuid();
        var vehicle = BuildVehicle(id);
        var (db, mockVehicles) = BuildContext([vehicle]);

        var useCase = new DeleteVehicleUseCase(db.Object);
        await useCase.ExecuteAsync(id);

        mockVehicles.Verify(m => m.Remove(vehicle), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var (db, _) = BuildContext();

        var useCase = new DeleteVehicleUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
