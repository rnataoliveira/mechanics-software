using FluentAssertions;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class StartDiagnosisUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReceivedOrder_StatusBecomesInDiagnosis()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var result = await new StartDiagnosisUseCase(db).ExecuteAsync(order.Id);

        result.Status.Should().Be("IN_DIAGNOSIS");
    }

    [Fact]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new StartDiagnosisUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WrongStatus_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        order.StartDiagnosis();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var act = async () => await new StartDiagnosisUseCase(db).ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
    }
}
