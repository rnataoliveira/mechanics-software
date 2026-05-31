using FluentAssertions;
using MechanicsSoftware.Application.Features.Services;
using MechanicsSoftware.UnitTests.Helpers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Services;

public class ListServicesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAllServicesOrderedByName()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Services.Add(Service.Create(Guid.NewGuid(), "Wheel Alignment", null, new Money(8000), 60));
        db.Services.Add(Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30));
        await db.SaveChangesAsync();

        var result = await new ListServicesUseCase(db).ExecuteAsync(new ListServicesQuery());

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Oil Change");
        result[1].Name.Should().Be("Wheel Alignment");
    }

    [Fact]
    public async Task ExecuteAsync_NameFilter_ReturnsMatchingServices()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Services.Add(Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30));
        db.Services.Add(Service.Create(Guid.NewGuid(), "Tire Rotation", null, new Money(3000), 45));
        await db.SaveChangesAsync();

        var result = await new ListServicesUseCase(db).ExecuteAsync(new ListServicesQuery(Name: "Oil"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Oil Change");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyNameFilter_ReturnsAll()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Services.Add(Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30));
        await db.SaveChangesAsync();

        var result = await new ListServicesUseCase(db).ExecuteAsync(new ListServicesQuery(Name: "  "));

        result.Should().HaveCount(1);
    }
}
