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

public class DeleteServiceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ExistingService_RemovesService()
    {
        await using var db = InMemoryDbContextHelper.Create();
        var service = Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30);
        db.Services.Add(service);
        await db.SaveChangesAsync();

        await new DeleteServiceHandler(db).ExecuteAsync(service.Id);

        db.Services.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new DeleteServiceHandler(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
