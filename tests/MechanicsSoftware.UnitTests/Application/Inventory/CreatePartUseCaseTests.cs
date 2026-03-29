using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class CreatePartUseCaseTests
{
    private static (Mock<IAppDbContext> db, Mock<Microsoft.EntityFrameworkCore.DbSet<Part>> mockParts)
        BuildContext(List<Part>? parts = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockParts = MockDbSetHelper.CreateMockDbSet(parts ?? []);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return (db, mockParts);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_CreatesPart()
    {
        var (db, mockParts) = BuildContext();
        var input = new CreatePartInput("ENG-001", "Oil Filter", null, 2500, 10);

        var useCase = new CreatePartUseCase(db.Object);
        var result = await useCase.ExecuteAsync(input);

        result.Code.Should().Be("ENG-001");
        result.Name.Should().Be("Oil Filter");
        result.StockQuantity.Should().Be(10);
        mockParts.Verify(m => m.Add(It.IsAny<Part>()), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateCode_ThrowsDomainException()
    {
        var existing = Part.Create(Guid.NewGuid(), "ENG-001", "Old Filter", null, new Money(1000));
        var (db, _) = BuildContext([existing]);
        var input = new CreatePartInput("ENG-001", "New Filter", null, 2500, 0);

        var useCase = new CreatePartUseCase(db.Object);
        var act = async () => await useCase.ExecuteAsync(input);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*ENG-001*");
    }
}
