using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class GetAverageExecutionTimeUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_NoCompletedOrders_ReturnsZero()
    {
        var orders = new List<ServiceOrder>
        {
            ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
        };

        var mockOrders = MockDbSetHelper.CreateMockDbSet(orders);
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);

        var result = await new GetAverageExecutionTimeUseCase(db.Object).ExecuteAsync();

        result.AverageHours.Should().Be(0);
        result.OrderCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedOrders_ReturnsAverage()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        order.SendBudget();
        order.Approve();
        order.Complete();

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { order });
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);

        var result = await new GetAverageExecutionTimeUseCase(db.Object).ExecuteAsync();

        result.OrderCount.Should().Be(1);
        result.AverageHours.Should().BeGreaterThanOrEqualTo(0);
    }
}
