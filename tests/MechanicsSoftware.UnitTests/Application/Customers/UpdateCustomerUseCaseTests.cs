using FluentAssertions;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Customers;
using MechanicsSoftware.Application.UseCases.Customers.Commands;
using MechanicsSoftware.Application.UseCases.Customers.Handlers;
using MechanicsSoftware.Application.UseCases.Customers.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Customers;

public class UpdateCustomerUseCaseTests
{
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidPhone = "11999999999";

    private static Customer BuildCustomer(Guid? id = null) =>
        Customer.Create(id ?? Guid.NewGuid(), "Original Name", ValidCpf, PersonType.INDIVIDUAL, "original@email.com", ValidPhone);

    private static Mock<IAppDbContext> BuildContext(List<Customer>? customers = null)
    {
        var db = new Mock<IAppDbContext>();
        db.Setup(d => d.Customers).Returns(MockDbSetHelper.CreateMockDbSet(customers ?? []).Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingCustomer_UpdatesAndReturnsResponse()
    {
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        var db = BuildContext([customer]);
        var request = new UpdateCustomerCommand("New Name", "new@email.com", "11888888888");

        var result = await new UpdateCustomerHandler(db.Object).ExecuteAsync(customerId, request);

        result.Name.Should().Be("New Name");
        result.Email.Should().Be("new@email.com");
        result.Phone.Should().Be("11888888888");
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();
        var request = new UpdateCustomerCommand("New Name", "new@email.com", ValidPhone);

        var act = async () => await new UpdateCustomerHandler(db.Object).ExecuteAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_InvalidEmail_ThrowsDomainException()
    {
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        var db = BuildContext([customer]);
        var request = new UpdateCustomerCommand("New Name", "not-an-email", ValidPhone);

        var act = async () => await new UpdateCustomerHandler(db.Object).ExecuteAsync(customerId, request);

        await act.Should().ThrowAsync<DomainException>();
    }
}
