using FluentAssertions;
using MechanicsSoftware.Application.UseCases.ServiceOrders;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Commands;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class ListServiceOrdersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAllOrders()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.ServiceOrders.Add(ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        db.ServiceOrders.Add(ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        await db.SaveChangesAsync();

        var result = await new ListServiceOrdersHandler(db).ExecuteAsync(new ListServiceOrdersQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_StatusFilter_ReturnsMatchingOrders()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var received = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var inDiagnosis = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        inDiagnosis.StartDiagnosis();
        db.ServiceOrders.Add(received);
        db.ServiceOrders.Add(inDiagnosis);
        await db.SaveChangesAsync();

        var result = await new ListServiceOrdersHandler(db).ExecuteAsync(
            new ListServiceOrdersQuery(Status: "RECEIVED"));

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("RECEIVED");
    }

    [Fact]
    public async Task ExecuteAsync_InvalidStatusFilter_ReturnsAllOrders()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.ServiceOrders.Add(ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        await db.SaveChangesAsync();

        var result = await new ListServiceOrdersHandler(db).ExecuteAsync(
            new ListServiceOrdersQuery(Status: "UNKNOWN_STATUS"));

        result.Should().HaveCount(1);
    }
}
