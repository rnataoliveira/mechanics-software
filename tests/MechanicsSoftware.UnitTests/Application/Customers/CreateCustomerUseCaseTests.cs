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

public class CreateCustomerUseCaseTests
{
    private const string ValidName  = "Joel Silva";
    private const string ValidCpf   = "529.982.247-25";
    private const string ValidEmail = "joel@email.com";
    private const string ValidPhone = "11999999999";

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
    public async Task ExecuteAsync_ValidRequest_ReturnsCustomerResponse()
    {
        var (db, mockCustomers) = BuildContext();

        var useCase = new CreateCustomerUseCase(db.Object);
        var request = new CreateCustomerRequest(ValidName, ValidCpf, PersonType.INDIVIDUAL, ValidEmail, ValidPhone);

        var result = await useCase.ExecuteAsync(request);

        result.Name.Should().Be(ValidName);
        result.Email.Should().Be(ValidEmail.ToLowerInvariant());
        result.Phone.Should().Be(ValidPhone);
        result.DocumentValue.Should().Be("52998224725");
        result.Id.Should().NotBeEmpty();
        mockCustomers.Verify(m => m.Add(It.IsAny<Customer>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateDocument_ThrowsDomainException()
    {
        var existingCustomer = Customer.Create(
            Guid.NewGuid(), ValidName, ValidCpf, PersonType.INDIVIDUAL, ValidEmail, ValidPhone);
        var (db, _) = BuildContext(customers: [existingCustomer]);

        var useCase = new CreateCustomerUseCase(db.Object);
        var request = new CreateCustomerRequest("Outro Cliente", ValidCpf, PersonType.INDIVIDUAL, "outro@email.com", "11888888888");

        var act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<DomainException>().WithMessage($"*{ValidCpf}*");
    }
}