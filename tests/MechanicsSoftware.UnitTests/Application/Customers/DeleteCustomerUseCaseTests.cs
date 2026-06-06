using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

public class DeleteCustomerUseCaseTests
{
    private const string ValidName  = "Joel Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidEmail = "joel@email.com";
    private const string ValidPhone = "11999999999";

    private static Customer BuildCustomer(Guid? id = null) =>
        Customer.Create(id ?? Guid.NewGuid(), ValidName, ValidCpf, PersonType.INDIVIDUAL, ValidEmail, ValidPhone);

    private static (Mock<IAppDbContext> db, Mock<DbSet<Customer>> customers)
        BuildContext(List<Customer>? customers = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockCustomers = MockDbSetHelper.CreateMockDbSet(customers ?? []);

        db.Setup(d => d.Customers).Returns(mockCustomers.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (db, mockCustomers);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingCustomer_RemovesAndSaves()
    {
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        var (db, mockCustomers) = BuildContext(customers: [customer]);

        var useCase = new DeleteCustomerHandler(db.Object);

        await useCase.ExecuteAsync(customerId);

        mockCustomers.Verify(m => m.Remove(customer), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var (db, _) = BuildContext();

        var useCase = new DeleteCustomerHandler(db.Object);

        var act = async () => await useCase.ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}
