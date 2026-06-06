using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.UnitTests.Helpers;
using Moq;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.UnitTests.Application.Inventory;

public class DeletePartUseCaseTests
{
    private static Part BuildPart(Guid? id = null, int initialStock = 0) =>
        Part.Create(id ?? Guid.NewGuid(), "BOLT-001", "Hex Bolt", null, new Money(50), initialStock);

    private static (Mock<IAppDbContext> db, Mock<DbSet<Part>> parts) BuildContext(Part? part)
    {
        var db = new Mock<IAppDbContext>();
        List<Part> list = part is null ? [] : [part];
        var mockParts = MockDbSetHelper.CreateMockDbSet(list);

        db.Setup(d => d.Parts).Returns(mockParts.Object);
        db.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockParts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                 .Returns(new ValueTask<Part?>(part));

        return (db, mockParts);
    }

    [Fact]
    public async Task ExecuteAsync_PartWithNoReservations_RemovesAndSaves()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId);
        var (db, mockParts) = BuildContext(part);

        await new DeletePartUseCase(db.Object).ExecuteAsync(partId);

        mockParts.Verify(m => m.Remove(part), Times.Once);
        db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_PartNotFound_ThrowsNotFoundException()
    {
        var (db, _) = BuildContext(null);

        var act = async () => await new DeletePartUseCase(db.Object).ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_PartWithPendingReservations_ThrowsDomainException()
    {
        var partId = Guid.NewGuid();
        var part = BuildPart(partId, initialStock: 5);
        part.Reserve(3, Guid.NewGuid());
        var (db, _) = BuildContext(part);

        var act = async () => await new DeletePartUseCase(db.Object).ExecuteAsync(partId);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*pending reservations*");
    }
}
