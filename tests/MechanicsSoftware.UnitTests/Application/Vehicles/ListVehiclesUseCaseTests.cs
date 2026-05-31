using FluentAssertions;
using MechanicsSoftware.Application.UseCases.Vehicles;
using MechanicsSoftware.Application.UseCases.Vehicles.Commands;
using MechanicsSoftware.Application.UseCases.Vehicles.Handlers;
using MechanicsSoftware.Application.UseCases.Vehicles.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Vehicles;

public class ListVehiclesUseCaseTests
{
    private static readonly Guid CustomerA = Guid.NewGuid();
    private static readonly Guid CustomerB = Guid.NewGuid();

    private static Vehicle BuildVehicle(string plate, Guid customerId) =>
        Vehicle.Create(Guid.NewGuid(), new LicensePlate(plate), "Toyota", "Corolla", 2020, customerId);

    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAll()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Vehicles.AddRange(
            BuildVehicle("ABC1234", CustomerA),
            BuildVehicle("XYZ5678", CustomerB));
        await db.SaveChangesAsync();

        var result = await new ListVehiclesHandler(db).ExecuteAsync(new ListVehiclesQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByCustomerId_ReturnsOnlyThatCustomerVehicles()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Vehicles.AddRange(
            BuildVehicle("ABC1234", CustomerA),
            BuildVehicle("DEF4321", CustomerA),
            BuildVehicle("XYZ5678", CustomerB));
        await db.SaveChangesAsync();

        var result = await new ListVehiclesHandler(db).ExecuteAsync(new ListVehiclesQuery(CustomerId: CustomerA));

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(v => v.CustomerId.Should().Be(CustomerA));
    }

    [Fact]
    public async Task ExecuteAsync_FilterByLicensePlate_ReturnsMatching()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Vehicles.AddRange(
            BuildVehicle("ABC1234", CustomerA),
            BuildVehicle("XYZ5678", CustomerB));
        await db.SaveChangesAsync();

        var result = await new ListVehiclesHandler(db).ExecuteAsync(new ListVehiclesQuery(LicensePlate: "abc1234"));

        result.Should().HaveCount(1);
        result[0].LicensePlate.Should().Be("ABC1234");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDatabase_ReturnsEmpty()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var result = await new ListVehiclesHandler(db).ExecuteAsync(new ListVehiclesQuery());

        result.Should().BeEmpty();
    }
}
