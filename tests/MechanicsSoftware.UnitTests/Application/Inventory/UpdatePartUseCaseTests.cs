using FluentAssertions;
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
        Part.Create(id ?? Guid.NewGuid(), "ENG-001", "Oil Filter", null, new Money(2500));

    private static Mock<IAppDbContext> BuildContext(Part? part = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockParts = MockDbSetHelper.CreateMockDbSet(part is null ? [] : [part]);
        mockParts
            .Setup(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(part);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_UpdatesAndReturnsOutput()
    {
        var id = Guid.NewGuid();
        var part = BuildPart(id);
        var db = BuildContext(part);
        var input = new UpdatePartInput("New Name", "New description", 5000);

        var useCase = new UpdatePartUseCase(db.Object);
        var result = await useCase.ExecuteAsync(id, input);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New description");
        result.UnitPriceInCents.Should().Be(5000);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();

        var useCase = new UpdatePartUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid(), new UpdatePartInput("Name", null, 1000));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
