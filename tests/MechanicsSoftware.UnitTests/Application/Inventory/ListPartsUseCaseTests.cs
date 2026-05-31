using FluentAssertions;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.UnitTests.Helpers;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class ListPartsUseCaseTests
{
    private static Part BuildPart(string code, string name) =>
        Part.Create(Guid.NewGuid(), code, name, null, new Money(1000), 5);

    [Fact]
    public async Task ExecuteAsync_NoFilter_ReturnsAll()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Parts.AddRange(
            BuildPart("OIL-001", "Engine Oil"),
            BuildPart("BOLT-001", "Hex Bolt"));
        await db.SaveChangesAsync();

        var result = await new ListPartsUseCase(db).ExecuteAsync(new ListPartsQuery());

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_FilterByCode_ReturnsMatching()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Parts.AddRange(
            BuildPart("OIL-001", "Engine Oil"),
            BuildPart("BOLT-001", "Hex Bolt"));
        await db.SaveChangesAsync();

        var result = await new ListPartsUseCase(db).ExecuteAsync(new ListPartsQuery(Code: "OIL"));

        result.Should().HaveCount(1);
        result.First().Code.Should().Be("OIL-001");
    }

    [Fact]
    public async Task ExecuteAsync_FilterByName_ReturnsMatching()
    {
        await using var db = InMemoryDbContextHelper.Create();
        db.Parts.AddRange(
            BuildPart("OIL-001", "Engine Oil"),
            BuildPart("BOLT-001", "Hex Bolt"));
        await db.SaveChangesAsync();

        var result = await new ListPartsUseCase(db).ExecuteAsync(new ListPartsQuery(Name: "Bolt"));

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Hex Bolt");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDatabase_ReturnsEmpty()
    {
        await using var db = InMemoryDbContextHelper.Create();

        var result = await new ListPartsUseCase(db).ExecuteAsync(new ListPartsQuery());

        result.Should().BeEmpty();
    }
}
