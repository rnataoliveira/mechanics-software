using FluentAssertions;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Shared;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }

    private sealed class OtherEntity : Entity<Guid>
    {
        public OtherEntity(Guid id) : base(id) { }
    }

    private static TestEntity New(Guid? id = null) => new(id ?? Guid.NewGuid());

    [Fact]
    public void Constructor_SetsId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var e = New();
        e.Equals(e).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameTypeAndId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        New(id).Equals(New(id)).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameTypeDifferentId_ReturnsFalse()
    {
        New().Equals(New()).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        new TestEntity(id).Equals(new OtherEntity(id)).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        New().Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_NonEntityObject_ReturnsFalse()
    {
        New().Equals("not an entity").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSameHash()
    {
        var id = Guid.NewGuid();
        New(id).GetHashCode().Should().Be(New(id).GetHashCode());
    }

    [Fact]
    public void OperatorEqual_BothNull_ReturnsTrue()
    {
        TestEntity? a = null, b = null;
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void OperatorEqual_LeftNull_ReturnsFalse()
    {
        TestEntity? a = null;
        (a == New()).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEqual_DifferentIds_ReturnsTrue()
    {
        (New() != New()).Should().BeTrue();
    }
}
