using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Vehicles;
using MechanicsSoftware.Domain.Vehicles;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Vehicles;

public class UpdateVehicleUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly LicensePlate ValidPlate = new("ABC1234");

    private static Vehicle BuildVehicle(Guid? id = null) =>
        Vehicle.Create(id ?? Guid.NewGuid(), ValidPlate, "Toyota", "Corolla", 2020, CustomerId);

    private static Mock<IAppDbContext> BuildContext(List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockVehicles = MockDbSetHelper.CreateMockDbSet(vehicles ?? []);
        db.Setup(d => d.Vehicles).Returns(mockVehicles.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_UpdatesAndReturnsResponse()
    {
        var id = Guid.NewGuid();
        var vehicle = BuildVehicle(id);
        var db = BuildContext([vehicle]);

        var useCase = new UpdateVehicleUseCase(db.Object);
        var request = new UpdateVehicleRequest(id, "Honda", "Civic", 2021);
        var result = await useCase.ExecuteAsync(request);

        result.Make.Should().Be("Honda");
        result.Model.Should().Be("Civic");
        result.Year.Should().Be(2021);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();

        var useCase = new UpdateVehicleUseCase(db.Object);
        var request = new UpdateVehicleRequest(Guid.NewGuid(), "Honda", "Civic", 2021);
        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
