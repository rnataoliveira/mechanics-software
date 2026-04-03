using FluentAssertions;
using MechanicsSoftware.Application.Features.Customers;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;

namespace MechanicsSoftware.UnitTests.Application.Customers;

public class ListCustomersUseCaseTests
{
    private static Customer BuildCustomer(string name, string cpf, string email) =>
        Customer.Create(Guid.NewGuid(), name, cpf, PersonType.INDIVIDUAL, email, "11999999999");

    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAll()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Customers.AddRange(
            BuildCustomer("Ana Silva",   "529.982.247-25", "ana@email.com"),
            BuildCustomer("Bruno Costa", "111.444.777-35", "bruno@email.com"));
        await db.SaveChangesAsync();

        var result = await new ListCustomersUseCase(db).ExecuteAsync(new ListCustomersQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByName_ReturnsMatching()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Customers.AddRange(
            BuildCustomer("Ana Silva",   "529.982.247-25", "ana@email.com"),
            BuildCustomer("Bruno Costa", "111.444.777-35", "bruno@email.com"));
        await db.SaveChangesAsync();

        var result = await new ListCustomersUseCase(db).ExecuteAsync(new ListCustomersQuery(Name: "Ana"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Ana Silva");
    }

    [Fact]
    public async Task ExecuteAsync_FilterByDocument_ReturnsMatching()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Customers.AddRange(
            BuildCustomer("Ana Silva",   "529.982.247-25", "ana@email.com"),
            BuildCustomer("Bruno Costa", "111.444.777-35", "bruno@email.com"));
        await db.SaveChangesAsync();

        var result = await new ListCustomersUseCase(db).ExecuteAsync(new ListCustomersQuery(Document: "52998224725"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Ana Silva");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDatabase_ReturnsEmpty()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var result = await new ListCustomersUseCase(db).ExecuteAsync(new ListCustomersQuery());

        result.Should().BeEmpty();
    }
}
