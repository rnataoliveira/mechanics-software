using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Abstractions;
using MechanicsSoftware.Application.Exceptions;
using MechanicsSoftware.Application.UseCases.Inventory;
using MechanicsSoftware.Application.UseCases.Inventory.Commands;
using MechanicsSoftware.Application.UseCases.Inventory.Handlers;
using MechanicsSoftware.Application.UseCases.Inventory.Queries;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class GetPartUseCaseTests
{
    private static Part BuildPart(Guid? id = null) =>
        Part.Create(id ?? Guid.NewGuid(), "OIL-001", "Engine Oil", "5W-30", new Money(2500), 10);

    private static Mock<IAppDbContext> BuildContext(Part? part)
    {
        var db = new Mock<IAppDbContext>();
        List<Part> list = part is null ? [] : [part];
        var mockParts = MockDbSetHelper.CreateMockDbSet(list);

        db.Setup(d => d.Parts).Returns(mockParts.Object);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingPart_ReturnsPartOutput()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var db = BuildContext(part);

        var result = await new GetPartHandler(db.Object).ExecuteAsync(partId);

        result.Id.Should().Be(partId);
        result.Code.Should().Be("OIL-001");
        result.Name.Should().Be("Engine Oil");
        result.UnitPriceInCents.Should().Be(2500);
        result.StockQuantity.Should().Be(10);
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var db = BuildContext(null);

        var act = async () => await new GetPartHandler(db.Object).ExecuteAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{nonExistentId}*");
    }
}
