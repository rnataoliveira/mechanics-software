using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class ListPartsUseCaseTests
{
    private static Part BuildPart(string code, string name) =>
        Part.Create(Guid.NewGuid(), code, name, null, new Money(1000));

    private static Mock<IAppDbContext> BuildContext(List<Part>? parts = null)
    {
        var db = new Mock<IAppDbContext>();
        var mockParts = MockDbSetHelper.CreateMockDbSet(parts ?? []);
        db.Setup(d => d.Parts).Returns(mockParts.Object);
        return db;
    }

    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAll()
    {
        var parts = new List<Part>
        {
            BuildPart("ENG-001", "Oil Filter"),
            BuildPart("BRK-001", "Brake Pad")
        };
        var db = BuildContext(parts);

        var useCase = new ListPartsUseCase(db.Object);
        var result = await useCase.ExecuteAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByCode_ReturnsFiltered()
    {
        var parts = new List<Part>
        {
            BuildPart("ENG-001", "Oil Filter"),
            BuildPart("BRK-001", "Brake Pad")
        };
        var db = BuildContext(parts);

        var useCase = new ListPartsUseCase(db.Object);
        var result = await useCase.ExecuteAsync(code: "ENG");

        result.Should().HaveCount(1);
        result.First().Code.Should().Be("ENG-001");
    }

    [Fact]
    public async Task ExecuteAsync_FilterByName_ReturnsFiltered()
    {
        var parts = new List<Part>
        {
            BuildPart("ENG-001", "Oil Filter"),
            BuildPart("BRK-001", "Brake Pad")
        };
        var db = BuildContext(parts);

        var useCase = new ListPartsUseCase(db.Object);
        var result = await useCase.ExecuteAsync(name: "Brake");

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Brake Pad");
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ReturnsEmpty()
    {
        var db = BuildContext();

        var useCase = new ListPartsUseCase(db.Object);
        var result = await useCase.ExecuteAsync();

        result.Should().BeEmpty();
    }
}
