using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Customers;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Customers;

public class GetCustomerUseCaseTests
{
    private const string ValidName  = "Joel Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidEmail = "joel@email.com";
    private const string ValidPhone = "11999999999";

    private static Customer BuildCustomer(Guid? id = null) =>
        Customer.Create(id ?? Guid.NewGuid(), ValidName, ValidCpf, PersonType.INDIVIDUAL, ValidEmail, ValidPhone);

    private static Mock<IAppDbContext> BuildContext(List<Customer>? customers = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockCustomers = MockDbSetHelper.CreateMockDbSet(customers ?? []);

        db.Setup(d => d.Customers).Returns(mockCustomers.Object);

        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingCustomer_ReturnsCustomerResponse()
    {
        var customerId = Guid.NewGuid();
        var customer = BuildCustomer(customerId);
        var db = BuildContext(customers: [customer]);

        var useCase = new GetCustomerUseCase(db.Object);

        var result = await useCase.ExecuteAsync(customerId);

        result.Id.Should().Be(customerId);
        result.Name.Should().Be(ValidName);
        result.Email.Should().Be(ValidEmail.ToLowerInvariant());
        result.Phone.Should().Be(ValidPhone);
    }

    [Fact]
    public async Task ExecuteAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var db = BuildContext();

        var useCase = new GetCustomerUseCase(db.Object);

        var act = async () => await useCase.ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}