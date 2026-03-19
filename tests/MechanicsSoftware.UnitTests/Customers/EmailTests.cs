using FluentAssertions;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.UnitTests.Customers;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@sub.domain.org")]
    public void Constructor_ValidEmail_Accepted(string email)
    {
        var e = new Email(email);
        e.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("USER@EXAMPLE.COM", "user@example.com")]
    [InlineData("User.Name@Domain.Com", "user.name@domain.com")]
    public void Constructor_UppercaseInput_NormalizedToLowercase(string input, string expected)
    {
        var e = new Email(input);
        e.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("userexample.com")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user@example")]
    public void Constructor_InvalidFormat_ThrowsDomainException(string email)
    {
        var act = () => new Email(email);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyOrWhitespace_ThrowsDomainException(string email)
    {
        var act = () => new Email(email);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equality_SameEmail_AreEqual()
    {
        var a = new Email("user@example.com");
        var b = new Email("user@example.com");
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentEmails_AreNotEqual()
    {
        var a = new Email("user@example.com");
        var b = new Email("other@example.com");
        (a != b).Should().BeTrue();
    }
}
