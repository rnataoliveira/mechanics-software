using FluentAssertions;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.ServiceOrders;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

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

        var mockOrders = MockDbSetHelper.CreateMockDbSet([order]);
        var mockServices = MockDbSetHelper.CreateMockDbSet([service]);
        mockServices.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .Returns(new ValueTask<Service?>(service));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Services).Returns(mockServices.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new AddServiceItemCommand(service.Id, 2);
        var result = await new AddServiceItemHandler(db.Object).ExecuteAsync(order.Id, request);

        result.ServiceId.Should().Be(service.Id);
        result.Quantity.Should().Be(2);
        result.UnitPriceInCents.Should().Be(5000);
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new AddServiceItemHandler(db).ExecuteAsync(
            Guid.NewGuid(), new AddServiceItemCommand(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ThrowsNotFoundException()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();

        var mockOrders = MockDbSetHelper.CreateMockDbSet([order]);
        var mockServices = MockDbSetHelper.CreateMockDbSet(new List<Service>());
        mockServices.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                    .Returns(new ValueTask<Service?>((Service?)null));

        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.ServiceOrders).Returns(mockOrders.Object);
        db.Setup(d => d.Services).Returns(mockServices.Object);

        var act = async () => await new AddServiceItemHandler(db.Object).ExecuteAsync(
            order.Id, new AddServiceItemCommand(Guid.NewGuid(), 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
