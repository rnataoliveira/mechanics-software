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

public class GetServiceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ExistingService_ReturnsResponse()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var service = Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30);
        db.Services.Add(service);
        await db.SaveChangesAsync();

        var result = await new GetServiceHandler(db).ExecuteAsync(service.Id);

        result.Id.Should().Be(service.Id);
        result.Name.Should().Be("Oil Change");
        result.BasePriceInCents.Should().Be(5000);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new GetServiceHandler(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
