using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class GetPartUseCaseTests
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
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_ValidId_ReturnsPartOutput()
    {
        var id = Guid.NewGuid();
        var part = BuildPart(id);
        var db = BuildContext(part);

        var useCase = new GetPartUseCase(db.Object);
        var result = await useCase.ExecuteAsync(id);

        result.Id.Should().Be(id);
        result.Code.Should().Be("ENG-001");
        result.Name.Should().Be("Oil Filter");
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        var db = BuildContext();

        var useCase = new GetPartUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
