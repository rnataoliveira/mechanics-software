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

public class UpdatePartUseCaseTests
{
    private static Part BuildPart(Guid? id = null) =>
        Part.Create(id ?? Guid.NewGuid(), "OIL-001", "Engine Oil", "5W-30", new Money(2500), 10);

    private static Mock<IAppDbContext> BuildContext(Part? part)
    {
        var db = new Mock<IAppDbContext>();
        List<Part> list = part is null ? [] : [part];
        var mockParts = MockDbSetHelper.CreateMockDbSet(list);

        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingPart_UpdatesAndReturnsOutput()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var db = BuildContext(part);
        var input = new UpdatePartCommand("Synthetic Oil", "Full synthetic", 3500);

        var result = await new UpdatePartHandler(db.Object).ExecuteAsync(partId, input);

        result.Name.Should().Be("Synthetic Oil");
        result.Description.Should().Be("Full synthetic");
        result.UnitPriceInCents.Should().Be(3500);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var db = BuildContext(null);
        var input = new UpdatePartCommand("Synthetic Oil", null, 3500);

        var act = async () => await new UpdatePartHandler(db.Object).ExecuteAsync(Guid.NewGuid(), input);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
