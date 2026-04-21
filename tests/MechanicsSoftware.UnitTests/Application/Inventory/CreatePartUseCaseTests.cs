using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class CreatePartUseCaseTests
{
    private static (Mock<IAppDbContext> db, Mock<DbSet<Part>> parts)
        BuildContext(List<Part>? parts = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockParts = MockDbSetHelper.CreateMockDbSet(parts ?? []);

        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return (db, mockParts);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_CreatesAndReturnsPart()
    {
        var (db, mockParts) = BuildContext();
        var input = new CreatePartRequest("OIL-001", "Engine Oil", "5W-30", 2500, 10);

        var result = await new CreatePartUseCase(db.Object).ExecuteAsync(input);

        result.Code.Should().Be("OIL-001");
        result.Name.Should().Be("Engine Oil");
        result.UnitPriceInCents.Should().Be(2500);
        result.StockQuantity.Should().Be(10);
        result.Id.Should().NotBeEmpty();
        mockParts.Verify(m => m.Add(It.IsAny<Part>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateCode_ThrowsDomainException()
    {
        var existing = Part.Create(Guid.NewGuid(), "OIL-001", "Engine Oil", null, new Money(2500), 5);
        var (db, _) = BuildContext([existing]);
        var input = new CreatePartRequest("OIL-001", "Another Oil", null, 1000, 0);

        var act = async () => await new CreatePartUseCase(db.Object).ExecuteAsync(input);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*OIL-001*");
    }

    [Fact]
    public async Task ExecuteAsync_ZeroInitialStock_CreatesWithoutMovement()
    {
        var (db, _) = BuildContext();
        var input = new CreatePartRequest("BOLT-001", "Hex Bolt", null, 50, 0);

        var result = await new CreatePartUseCase(db.Object).ExecuteAsync(input);

        result.StockQuantity.Should().Be(0);
    }
}
