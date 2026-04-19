using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class ListServiceOrdersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAllOrders()
    {
        var orders = new List<ServiceOrder>
        {
            ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
        };

        var mockOrders = MockDbSetHelper.CreateMockDbSet(orders);
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);

        var result = await new ListServiceOrdersUseCase(db.Object).ExecuteAsync(new ListServiceOrdersQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_StatusFilter_ReturnsMatchingOrders()
    {
        var received = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var inDiagnosis = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        inDiagnosis.StartDiagnosis();

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { received, inDiagnosis });
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);

        var result = await new ListServiceOrdersUseCase(db.Object).ExecuteAsync(
            new ListServiceOrdersQuery(Status: "RECEIVED"));

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("RECEIVED");
    }

    [Fact]
    public async Task ExecuteAsync_InvalidStatusFilter_ReturnsAllOrders()
    {
        var orders = new List<ServiceOrder>
        {
            ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
        };

        var mockOrders = MockDbSetHelper.CreateMockDbSet(orders);
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);

        var result = await new ListServiceOrdersUseCase(db.Object).ExecuteAsync(
            new ListServiceOrdersQuery(Status: "UNKNOWN_STATUS"));

        result.Should().HaveCount(1);
    }
}
