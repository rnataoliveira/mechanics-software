using FluentAssertions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Services;
using MechanicsSoftware.Application.UseCases.Services.Commands;
using MechanicsSoftware.Application.UseCases.Services.Handlers;
using MechanicsSoftware.Application.UseCases.Services.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Services;

public class UpdateServiceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsUpdatedResponse()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var service = Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30);
        db.Services.Add(service);
        await db.SaveChangesAsync();

        var request = new UpdateServiceCommand("Oil Change Pro", "Full synthetic", 7500, 45);
        var result = await new UpdateServiceHandler(db).ExecuteAsync(service.Id, request);

        result.Name.Should().Be("Oil Change Pro");
        result.BasePriceInCents.Should().Be(7500);
        result.EstimatedMinutes.Should().Be(45);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new UpdateServiceHandler(db).ExecuteAsync(
            Guid.NewGuid(), new UpdateServiceCommand("X", null, 1000, 30));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateName_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var existing = Service.Create(Guid.NewGuid(), "Tire Rotation", null, new Money(3000), 45);
        var target = Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30);
        db.Services.Add(existing);
        db.Services.Add(target);
        await db.SaveChangesAsync();

        var act = async () => await new UpdateServiceHandler(db).ExecuteAsync(
            target.Id, new UpdateServiceCommand("Tire Rotation", null, 5000, 30));

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Tire Rotation*");
    }
}
