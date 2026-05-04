using FluentAssertions;
using MechanicsSoftware.Infrastructure.Security;

namespace MechanicsSoftware.UnitTests.Infrastructure;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _hasher.Hash("secret123");

        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_TwoCallsSamePassword_ReturnsDifferentHashes()
    {
        var hash1 = _hasher.Hash("secret123");
        var hash2 = _hasher.Hash("secret123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("myPassword");

        _hasher.Verify("myPassword", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("myPassword");

        _hasher.Verify("wrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHash_ReturnsFalse()
    {
        _hasher.Verify("anyPassword", "not-a-valid-bcrypt-hash").Should().BeFalse();
    }

    [Fact]
    public void Constructor_ValidSaltRoundsEnvVar_HashesAndVerifies()
    {
        Environment.SetEnvironmentVariable("BCRYPT_SALT_ROUNDS", "4");
        try
        {
            var hasher = new BCryptPasswordHasher();
            var hash = hasher.Hash("test");
            hasher.Verify("test", hash).Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("BCRYPT_SALT_ROUNDS", null);
        }
    }
}
