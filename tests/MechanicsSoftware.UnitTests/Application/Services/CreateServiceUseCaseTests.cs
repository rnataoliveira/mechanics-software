using FluentAssertions;
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

public class CreateServiceUseCaseTests
{
    private static CreateServiceCommand ValidRequest() =>
        new("Oil Change", "Full engine oil change", 5000, 30);

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsServiceResponse()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var result = await new CreateServiceHandler(db).ExecuteAsync(ValidRequest());

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Oil Change");
        result.BasePriceInCents.Should().Be(5000);
        result.EstimatedMinutes.Should().Be(30);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateName_ThrowsDomainException()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Services.Add(Service.Create(Guid.NewGuid(), "Oil Change", null, new Money(5000), 30));
        await db.SaveChangesAsync();

        var act = async () => await new CreateServiceHandler(db).ExecuteAsync(ValidRequest());

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Oil Change*");
    }
}
