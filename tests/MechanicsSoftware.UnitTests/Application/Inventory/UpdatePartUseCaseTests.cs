using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class UpdatePartUseCaseTests
{
    private static Part BuildPart(Guid? id = null) =>
        Part.Create(id ?? Guid.NewGuid(), "OIL-001", "Engine Oil", "5W-30", new Money(2500), 10);

    private static Mock<IAppDbContext> BuildContext(Part? part)
    {
        var db = new Mock<IAppDbContext>();
        var list = part is null ? new List<Part>() : new List<Part> { part };
        var mockParts = MockDbSetHelper.CreateMockDbSet(list);

        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(ValueTask.FromResult<Part?>(part));

        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ExistingPart_UpdatesAndReturnsOutput()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var db = BuildContext(part);
        var input = new UpdatePartRequest("Synthetic Oil", "Full synthetic", 3500);

        var result = await new UpdatePartUseCase(db.Object).ExecuteAsync(partId, input);

        result.Name.Should().Be("Synthetic Oil");
        result.Description.Should().Be("Full synthetic");
        result.UnitPriceInCents.Should().Be(3500);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var db = BuildContext(null);
        var input = new UpdatePartRequest("Synthetic Oil", null, 3500);

        var act = async () => await new UpdatePartUseCase(db.Object).ExecuteAsync(Guid.NewGuid(), input);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
