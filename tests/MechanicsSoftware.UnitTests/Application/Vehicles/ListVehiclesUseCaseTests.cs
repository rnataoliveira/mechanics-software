using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.Vehicles;
using MechanicsSoftware.Domain.Vehicles;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Vehicles;

public class ListVehiclesUseCaseTests
{
    private static readonly Guid CustomerA = Guid.NewGuid();
    private static readonly Guid CustomerB = Guid.NewGuid();

    private static Vehicle Build(string plate, Guid customerId) =>
        Vehicle.Create(Guid.NewGuid(), new LicensePlate(plate), "Toyota", "Corolla", 2020, customerId);

    private static Mock<IAppDbContext> BuildContext(List<Vehicle>? vehicles = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockVehicles = MockDbSetHelper.CreateMockDbSet(vehicles ?? []);
        db.Setup(d => d.Vehicles).Returns(mockVehicles.Object);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAll()
    {
        var vehicles = new List<Vehicle>
        {
            Build("ABC1234", CustomerA),
            Build("XYZ5678", CustomerB)
        };
        var db = BuildContext(vehicles);

        var useCase = new ListVehiclesUseCase(db.Object);
        var result = await useCase.ExecuteAsync(new ListVehiclesQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByCustomerId_ReturnsFiltered()
    {
        var vehicles = new List<Vehicle>
        {
            Build("ABC1234", CustomerA),
            Build("XYZ5678", CustomerB)
        };
        var db = BuildContext(vehicles);

        var useCase = new ListVehiclesUseCase(db.Object);
        var result = await useCase.ExecuteAsync(new ListVehiclesQuery(CustomerId: CustomerA));

        result.Should().HaveCount(1);
        result[0].CustomerId.Should().Be(CustomerA);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByLicensePlate_ReturnsFiltered()
    {
        var vehicles = new List<Vehicle>
        {
            Build("ABC1234", CustomerA),
            Build("XYZ5678", CustomerB)
        };
        var db = BuildContext(vehicles);

        var useCase = new ListVehiclesUseCase(db.Object);
        var result = await useCase.ExecuteAsync(new ListVehiclesQuery(LicensePlate: "abc1234"));

        result.Should().HaveCount(1);
        result[0].LicensePlate.Should().Be("ABC1234");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsEmpty()
    {
        var db = BuildContext();

        var useCase = new ListVehiclesUseCase(db.Object);
        var result = await useCase.ExecuteAsync(new ListVehiclesQuery());

        result.Should().BeEmpty();
    }
}
