using FluentAssertions;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class StatusChangeEmailNotificationTests
{
    private static Customer BuildCustomer(Guid id) =>
        Customer.Create(id, "João Silva", "529.982.247-25", PersonType.INDIVIDUAL, "joao@example.com", "11999999999");

    private static ServiceOrder NewOrder(Guid customerId) =>
        ServiceOrder.Create(Guid.NewGuid(), customerId, Guid.NewGuid());

    private static ServiceOrder OrderInDiagnosisWithBudget(Guid customerId)
    {
        var order = NewOrder(customerId);
        order.StartDiagnosis();
        order.AddServiceItem(Guid.NewGuid(), "Oil Change", new Money(5000), 1);
        order.GenerateBudget();
        return order;
    }

    private static ServiceOrder OrderAwaitingApproval(Guid customerId)
    {
        var order = OrderInDiagnosisWithBudget(customerId);
        order.SendBudget();
        return order;
    }

    [Fact]
    public async Task StartDiagnosis_Success_NotifiesWithCorrectParameters()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        db.Customers.Add(customer);
        var order = NewOrder(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        await new StartDiagnosisHandler(db, emailNotifier.Object, HandlerStubs.Logger<StartDiagnosisHandler>())
            .ExecuteAsync(order.Id);

        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            customer.Email.Value,
            customer.Name,
            order.Id,
            ServiceOrderStatus.Status.InDiagnosis,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBudget_Success_NotifiesWithCorrectParameters()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        db.Customers.Add(customer);
        var order = OrderInDiagnosisWithBudget(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        await new SendBudgetHandler(db, emailNotifier.Object, HandlerStubs.Logger<SendBudgetHandler>())
            .ExecuteAsync(order.Id);

        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            customer.Email.Value,
            customer.Name,
            order.Id,
            ServiceOrderStatus.Status.AwaitingApproval,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_Success_NotifiesWithCorrectParameters()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        db.Customers.Add(customer);
        var order = OrderAwaitingApproval(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        await new ApproveServiceOrderHandler(db, emailNotifier.Object, HandlerStubs.Logger<ApproveServiceOrderHandler>())
            .ExecuteAsync(order.Id);

        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            customer.Email.Value,
            customer.Name,
            order.Id,
            ServiceOrderStatus.Status.InExecution,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartDiagnosis_NotifierThrows_StatusStillPersisted()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        db.Customers.Add(BuildCustomer(customerId));
        var order = NewOrder(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();
        emailNotifier
            .Setup(e => e.SendStatusChangedAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var act = async () => await new StartDiagnosisHandler(db, emailNotifier.Object, HandlerStubs.Logger<StartDiagnosisHandler>())
            .ExecuteAsync(order.Id);

        await act.Should().NotThrowAsync();

        var persisted = await db.ServiceOrders.FindAsync(order.Id);
        persisted!.Status.Value.Should().Be(ServiceOrderStatus.Status.InDiagnosis);
    }

    [Fact]
    public async Task SendBudget_NotifierThrows_StatusStillPersisted()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        db.Customers.Add(BuildCustomer(customerId));
        var order = OrderInDiagnosisWithBudget(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();
        emailNotifier
            .Setup(e => e.SendStatusChangedAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var act = async () => await new SendBudgetHandler(db, emailNotifier.Object, HandlerStubs.Logger<SendBudgetHandler>())
            .ExecuteAsync(order.Id);

        await act.Should().NotThrowAsync();

        var persisted = await db.ServiceOrders.FindAsync(order.Id);
        persisted!.Status.Value.Should().Be(ServiceOrderStatus.Status.AwaitingApproval);
    }

    [Fact]
    public async Task Approve_NotifierThrows_StatusStillPersisted()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        db.Customers.Add(BuildCustomer(customerId));
        var order = OrderAwaitingApproval(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();
        emailNotifier
            .Setup(e => e.SendStatusChangedAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var act = async () => await new ApproveServiceOrderHandler(db, emailNotifier.Object, HandlerStubs.Logger<ApproveServiceOrderHandler>())
            .ExecuteAsync(order.Id);

        await act.Should().NotThrowAsync();

        var persisted = await db.ServiceOrders.FindAsync(order.Id);
        persisted!.Status.Value.Should().Be(ServiceOrderStatus.Status.InExecution);
    }

    [Fact]
    public async Task StartDiagnosis_InvalidTransition_DoesNotNotify()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        db.Customers.Add(BuildCustomer(customerId));
        var order = NewOrder(customerId);
        order.StartDiagnosis();
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        var act = async () => await new StartDiagnosisHandler(db, emailNotifier.Object, HandlerStubs.Logger<StartDiagnosisHandler>())
            .ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
            It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendBudget_InvalidTransition_DoesNotNotify()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        db.Customers.Add(BuildCustomer(customerId));
        var order = NewOrder(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        var act = async () => await new SendBudgetHandler(db, emailNotifier.Object, HandlerStubs.Logger<SendBudgetHandler>())
            .ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
            It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Approve_InvalidTransition_DoesNotNotify()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        db.Customers.Add(BuildCustomer(customerId));
        var order = NewOrder(customerId);
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        var act = async () => await new ApproveServiceOrderHandler(db, emailNotifier.Object, HandlerStubs.Logger<ApproveServiceOrderHandler>())
            .ExecuteAsync(order.Id);

        await act.Should().ThrowAsync<DomainException>();
        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
            It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
