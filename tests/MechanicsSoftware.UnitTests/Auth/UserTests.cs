using FluentAssertions;
using MechanicsSoftware.Domain.Auth;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Domain.Auth;

public class UserTests
{
    [Fact]
    public void Create_ValidArgs_ReturnsUser()
    {
        var id = Guid.NewGuid();
        var user = User.Create(id, "Test User", "test@email.com", "hashed_pwd", User.Roles.Mechanic);

        user.Id.Should().Be(id);
        user.Name.Should().Be("Test User");
        user.Email.Value.Should().Be("test@email.com");
        user.PasswordHash.Should().Be("hashed_pwd");
        user.Role.Should().Be(User.Roles.Mechanic);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsDomainException(string name)
    {
        var act = () => User.Create(Guid.NewGuid(), name, "test@email.com", "hashed_pwd", User.Roles.Mechanic);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyPasswordHash_ThrowsDomainException(string hash)
    {
        var act = () => User.Create(Guid.NewGuid(), "Test User", "test@email.com", hash, User.Roles.Mechanic);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NameIsTrimmed()
    {
        var user = User.Create(Guid.NewGuid(), "  Test User  ", "test@email.com", "hashed_pwd", User.Roles.Admin);
        user.Name.Should().Be("Test User");
    }
}
