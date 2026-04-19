using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Services;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class AddServiceItemUseCaseTests
{
    private static Service BuildService() =>
        Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30);

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsServiceItem()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        var service = BuildService();

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { order });
        var mockServices = MockDbSetHelper.CreateMockDbSet(new List<Service> { service });
        mockServices.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .Returns(ValueTask.FromResult<Service?>(service));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Services).Returns(mockServices.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new AddServiceItemRequest(service.Id, 2);
        var result = await new AddServiceItemUseCase(db.Object).ExecuteAsync(order.Id, request);

        result.ServiceId.Should().Be(service.Id);
        result.Quantity.Should().Be(2);
        result.UnitPriceInCents.Should().Be(5000);
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new AddServiceItemUseCase(db).ExecuteAsync(
            Guid.NewGuid(), new AddServiceItemRequest(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ThrowsNotFoundException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();

        var mockOrders = MockDbSetHelper.CreateMockDbSet(new List<ServiceOrder> { order });
        var mockServices = MockDbSetHelper.CreateMockDbSet(new List<Service>());
        mockServices.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .Returns(ValueTask.FromResult<Service?>(null));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Services).Returns(mockServices.Object);

        var act = async () => await new AddServiceItemUseCase(db.Object).ExecuteAsync(
            order.Id, new AddServiceItemRequest(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
