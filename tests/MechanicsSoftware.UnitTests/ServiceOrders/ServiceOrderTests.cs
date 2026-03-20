using FluentAssertions;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Domain.ServiceOrders;

public class ServiceOrderTests
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

    private static ServiceOrder AwaitingApproval()
    {
        var o = WithBudget();
        o.SendBudget();
        return o;
    }

    private static ServiceOrder InExecution()
    {
        var o = AwaitingApproval();
        o.Approve();
        return o;
    }

    [Fact]
    public void Create_ValidArgs_StatusIsReceived()
    {
        var order = NewOrder();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.Received);
    }

    [Fact]
    public void Create_EmptyCustomerId_Throws()
    {
        var act = () => ServiceOrder.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_EmptyVehicleId_Throws()
    {
        var act = () => ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StartDiagnosis_FromReceived_StatusIsInDiagnosis()
    {
        var order = NewOrder();
        order.StartDiagnosis();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.InDiagnosis);
    }

    [Fact]
    public void StartDiagnosis_AlreadyInDiagnosis_Throws()
    {
        var order = InDiagnosis();
        var act = () => order.StartDiagnosis();
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void AddServiceItem_InDiagnosis_AddsItem()
    {
        var order = InDiagnosis();
        var item = order.AddServiceItem(Guid.NewGuid(), "Alignment", new Money(8000), 1);
        order.ServiceItems.Should().HaveCount(1);
        item.ServiceName.Should().Be("Alignment");
        item.UnitPrice.Cents.Should().Be(8000);
    }

    [Fact]
    public void AddServiceItem_SnapshotsPrice()
    {
        var order = InDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Brake Job", new Money(15000), 2);
        order.ServiceItems.First().UnitPrice.Cents.Should().Be(15000);
    }

    [Fact]
    public void AddServiceItem_WhenNotInDiagnosis_Throws()
    {
        var order = NewOrder();
        var act = () => order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddServiceItem_ZeroQuantity_Throws()
    {
        var order = InDiagnosis();
        var act = () => order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 0);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddPartItem_Available_AddsItem()
    {
        var order = InDiagnosis();
        var item = order.AddPartItem(Guid.NewGuid(), "Oil Filter", new Money(2500), 1, PartAvailability.Available);
        order.PartItems.Should().HaveCount(1);
        item.Availability.Should().Be(PartAvailability.Available);
    }

    [Fact]
    public void AddPartItem_Unavailable_MarkedCorrectly()
    {
        var order = InDiagnosis();
        var item = order.AddPartItem(Guid.NewGuid(), "Rare Gasket", new Money(9000), 1, PartAvailability.Unavailable);
        item.Availability.Should().Be(PartAvailability.Unavailable);
    }

    [Fact]
    public void AddPartItem_WhenNotInDiagnosis_Throws()
    {
        var order = NewOrder();
        var act = () => order.AddPartItem(Guid.NewGuid(), "Filter", new Money(2500), 1, PartAvailability.Available);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GenerateBudget_WithAvailableService_CreatesBudget()
    {
        var order = InDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        var budget = order.GenerateBudget();
        budget.Should().NotBeNull();
        budget.Total.Cents.Should().Be(5000);
    }

    [Fact]
    public void GenerateBudget_UnavailablePartExcludedFromTotal()
    {
        var order = InDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.AddPartItem(Guid.NewGuid(), "Oil Filter", new Money(2000), 1, PartAvailability.Available);
        order.AddPartItem(Guid.NewGuid(), "Rare Part", new Money(9999), 1, PartAvailability.Unavailable);

        var budget = order.GenerateBudget();

        budget.Total.Cents.Should().Be(7000);
    }

    [Fact]
    public void GenerateBudget_NoServiceItems_Throws()
    {
        var order = InDiagnosis();
        order.AddPartItem(Guid.NewGuid(), "Filter", new Money(2000), 1, PartAvailability.Unavailable);
        var act = () => order.GenerateBudget();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GenerateBudget_WhenNotInDiagnosis_Throws()
    {
        var order = NewOrder();
        var act = () => order.GenerateBudget();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GenerateBudget_BudgetStatusIsPending()
    {
        var order = InDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Service A", new Money(1000), 1);
        var budget = order.GenerateBudget();
        budget.Status.Value.Should().Be(BudgetStatus.Status.Pending);
    }

    [Fact]
    public void SendBudget_WithBudget_StatusIsAwaitingApproval()
    {
        var order = WithBudget();
        order.SendBudget();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.AwaitingApproval);
    }

    [Fact]
    public void SendBudget_WithoutBudget_Throws()
    {
        var order = InDiagnosis();
        var act = () => order.SendBudget();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_FromAwaitingApproval_StatusIsInExecution()
    {
        var order = AwaitingApproval();
        order.Approve();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.InExecution);
    }

    [Fact]
    public void Approve_BudgetStatusBecomesApproved()
    {
        var order = AwaitingApproval();
        order.Approve();
        order.Budget!.Status.Value.Should().Be(BudgetStatus.Status.Approved);
    }

    [Fact]
    public void Approve_WhenNotAwaitingApproval_Throws()
    {
        var order = InDiagnosis();
        var act = () => order.Approve();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_FromAwaitingApproval_StatusIsCancelled()
    {
        var order = AwaitingApproval();
        order.Reject();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.Cancelled);
    }

    [Fact]
    public void Reject_BudgetStatusBecomesRejected()
    {
        var order = AwaitingApproval();
        order.Reject();
        order.Budget!.Status.Value.Should().Be(BudgetStatus.Status.Rejected);
    }

    [Fact]
    public void Reject_WhenNotAwaitingApproval_Throws()
    {
        var order = InDiagnosis();
        var act = () => order.Reject();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StartExecution_WhenInExecution_DoesNotThrow()
    {
        var order = InExecution();
        var act = () => order.StartExecution();
        act.Should().NotThrow();
    }

    [Fact]
    public void StartExecution_WhenNotInExecution_Throws()
    {
        var order = AwaitingApproval();
        var act = () => order.StartExecution();
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Complete_FromInExecution_StatusIsCompleted()
    {
        var order = InExecution();
        order.Complete();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.Completed);
    }

    [Fact]
    public void Complete_WhenNotInExecution_Throws()
    {
        var order = AwaitingApproval();
        var act = () => order.Complete();
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Deliver_FromCompleted_StatusIsDelivered()
    {
        var order = InExecution();
        order.Complete();
        order.Deliver();
        order.Status.Value.Should().Be(ServiceOrderStatus.Status.Delivered);
    }

    [Fact]
    public void Deliver_RecordsDeliveryTimestamp()
    {
        var order = InExecution();
        order.Complete();
        var before = DateTime.UtcNow;
        order.Deliver();
        order.DeliveredAt.Should().NotBeNull();
        order.DeliveredAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Deliver_WhenNotCompleted_Throws()
    {
        var order = InExecution();
        var act = () => order.Deliver();
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void FullLifecycle_HappyPath_AllTransitionsSucceed()
    {
        var order = NewOrder();

        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Full Service", new Money(20000), 1);
        order.AddPartItem(Guid.NewGuid(), "Air Filter", new Money(3000), 2, PartAvailability.Available);
        order.AddPartItem(Guid.NewGuid(), "Rare Sensor", new Money(50000), 1, PartAvailability.Unavailable);

        var budget = order.GenerateBudget();
        budget.Total.Cents.Should().Be(26000);

        order.SendBudget();
        order.Approve();
        order.StartExecution();
        order.Complete();
        order.Deliver();

        order.Status.Value.Should().Be(ServiceOrderStatus.Status.Delivered);
        order.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public void FullLifecycle_Rejection_StatusIsCancelled()
    {
        var order = NewOrder();
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Inspection", new Money(10000), 1);
        order.GenerateBudget();
        order.SendBudget();
        order.Reject();

        order.Status.Value.Should().Be(ServiceOrderStatus.Status.Cancelled);
        order.Budget!.Status.Value.Should().Be(BudgetStatus.Status.Rejected);
    }
}