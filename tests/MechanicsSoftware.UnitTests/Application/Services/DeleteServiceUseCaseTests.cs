using FluentAssertions;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Services;
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

        await new DeleteServiceUseCase(db).ExecuteAsync(service.Id);

        db.Services.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ServiceNotFound_ThrowsNotFoundException()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var act = async () => await new DeleteServiceUseCase(db).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
