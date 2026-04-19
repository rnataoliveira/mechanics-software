using FluentAssertions;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Infrastructure;

public class ServiceOrderConfigurationTests
{
    private static ServiceOrder NewOrder() =>
        ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    private static ServiceOrder InDiagnosis()
    {
        var o = NewOrder();
        o.StartDiagnosis();
        return o;
    }

    private static ServiceOrder WithBudget()
    {
        var o = InDiagnosis();
        o.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        o.GenerateBudget();
        return o;
    }

    private async Task<ServiceOrder> RoundTrip(ServiceOrder order)
    {
        await using var ctx = InMemoryDbContextHelper.Create();
        ctx.ServiceOrders.Add(order);
        await ctx.SaveChangesAsync();
        ctx.ChangeTracker.Clear();
        return (await ctx.ServiceOrders.FindAsync(order.Id))!;
    }

    [Fact]
    public async Task ServiceOrder_StatusReceived_RoundTrips()
    {
        var loaded = await RoundTrip(NewOrder());
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.Received);
    }

    [Fact]
    public async Task ServiceOrder_StatusInDiagnosis_RoundTrips()
    {
        var loaded = await RoundTrip(InDiagnosis());
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.InDiagnosis);
    }

    [Fact]
    public async Task ServiceOrder_StatusAwaitingApproval_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        var loaded = await RoundTrip(o);
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.AwaitingApproval);
    }

    [Fact]
    public async Task ServiceOrder_StatusInExecution_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        o.Approve();
        var loaded = await RoundTrip(o);
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.InExecution);
    }

    [Fact]
    public async Task ServiceOrder_StatusCompleted_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        o.Approve();
        o.Complete();
        var loaded = await RoundTrip(o);
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.Completed);
    }

    [Fact]
    public async Task ServiceOrder_StatusDelivered_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        o.Approve();
        o.Complete();
        o.Deliver();
        var loaded = await RoundTrip(o);
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.Delivered);
        loaded.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ServiceOrder_StatusCancelled_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        o.Reject();
        var loaded = await RoundTrip(o);
        loaded.Status.Value.Should().Be(ServiceOrderStatus.Status.Cancelled);
    }

    [Fact]
    public async Task Budget_StatusPending_RoundTrips()
    {
        var o = WithBudget();
        var loaded = await RoundTrip(o);
        loaded.Budget!.Status.Value.Should().Be(BudgetStatus.Status.Pending);
    }

    [Fact]
    public async Task Budget_StatusApproved_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        o.Approve();
        var loaded = await RoundTrip(o);
        loaded.Budget!.Status.Value.Should().Be(BudgetStatus.Status.Approved);
    }

    [Fact]
    public async Task Budget_StatusRejected_RoundTrips()
    {
        var o = WithBudget();
        o.SendBudget();
        o.Reject();
        var loaded = await RoundTrip(o);
        loaded.Budget!.Status.Value.Should().Be(BudgetStatus.Status.Rejected);
    }
}
