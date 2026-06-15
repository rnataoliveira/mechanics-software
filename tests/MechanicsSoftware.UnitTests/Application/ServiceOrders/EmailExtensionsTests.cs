using FluentAssertions;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.UseCases.ServiceOrders.Handlers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.ServiceOrders;

public class EmailExtensionsTests
{
    private static Customer BuildCustomer(Guid id) =>
        Customer.Create(id, "João Silva", "529.982.247-25", PersonType.INDIVIDUAL, "joao@example.com", "11999999999");

    [Fact]
    public async Task TrySendStatusEmailAsync_CustomerExists_SendsEmail()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        db.Customers.Add(customer);
        var order = ServiceOrder.Create(Guid.NewGuid(), customerId, Guid.NewGuid());
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();
        emailNotifier
            .Setup(e => e.SendStatusChangedAsync(
                customer.Email.Value, customer.Name, order.Id,
                It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await emailNotifier.Object.TrySendStatusEmailAsync(db, HandlerStubs.Logger<EmailExtensionsTests>(), order, CancellationToken.None);

        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            customer.Email.Value, customer.Name, order.Id,
            It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrySendStatusEmailAsync_CustomerNotFound_DoesNotThrow()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();

        var act = async () => await emailNotifier.Object.TrySendStatusEmailAsync(
            db, HandlerStubs.Logger<EmailExtensionsTests>(), order, CancellationToken.None);

        await act.Should().NotThrowAsync();
        emailNotifier.Verify(e => e.SendStatusChangedAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
            It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TrySendStatusEmailAsync_EmailThrows_DoesNotThrow()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        db.Customers.Add(customer);
        var order = ServiceOrder.Create(Guid.NewGuid(), customerId, Guid.NewGuid());
        db.ServiceOrders.Add(order);
        await db.SaveChangesAsync();

        var emailNotifier = new Mock<IEmailNotifier>();
        emailNotifier
            .Setup(e => e.SendStatusChangedAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus.Status>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));

        var act = async () => await emailNotifier.Object.TrySendStatusEmailAsync(
            db, HandlerStubs.Logger<EmailExtensionsTests>(), order, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
