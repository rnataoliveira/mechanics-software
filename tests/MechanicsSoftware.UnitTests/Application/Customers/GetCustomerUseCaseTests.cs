using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Customers;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Customers;

public class GetCustomerUseCaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private const string ValidName  = "Joel Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidEmail = "joel@email.com";
    private const string ValidPhone = "11999999999";

    private static Customer BuildCustomer(Guid? id = null) =>
        Customer.Create(id ?? CustomerId, ValidName, ValidCpf, PersonType.INDIVIDUAL, ValidEmail, ValidPhone);

    private static (Mock<IAppDbContext> db, Mock<DbSet<Customer>> customers)
        BuildContext(List<Customer>? customers = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockCustomers = MockDbSetHelper.CreateMockDbSet(customers ?? []);

        db.Setup(d => d.Customers).Returns(mockCustomers.Object);

        return (db, mockCustomers);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingCustomer_ReturnsCustomerResponse()
    {
        var customer = BuildCustomer();
        var (db, _) = BuildContext(customers: [customer]);

        var useCase = new GetCustomerUseCase(db.Object);

        var result = await useCase.ExecuteAsync(CustomerId);

        result.Id.Should().Be(CustomerId);
        result.Name.Should().Be(ValidName);
        result.Document.Should().Be(ValidCpf);
        result.Email.Should().Be(ValidEmail);
        result.Phone.Should().Be(ValidPhone);
        result.PersonType.Should().Be("INDIVIDUAL");
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentCustomer_ThrowsNotFoundException()
    {
        var (db, _) = BuildContext();
        var nonExistentId = Guid.NewGuid();

        var useCase = new GetCustomerUseCase(db.Object);

        var act = async () => await useCase.ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}