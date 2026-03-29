using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Vehicles;
using MechanicsSoftware.Domain.Vehicles;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Vehicles;

public class GetVehicleUseCaseTests
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
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_ReturnsVehicleResponse()
    {
        var id = Guid.NewGuid();
        var vehicle = BuildVehicle(id);
        var db = BuildContext([vehicle]);

        var useCase = new GetVehicleUseCase(db.Object);
        var result = await useCase.ExecuteAsync(id);

        result.Id.Should().Be(id);
        result.LicensePlate.Should().Be(ValidPlate.Value);
        result.Make.Should().Be("Toyota");
        result.Model.Should().Be("Corolla");
        result.CustomerId.Should().Be(CustomerId);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();

        var useCase = new GetVehicleUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
