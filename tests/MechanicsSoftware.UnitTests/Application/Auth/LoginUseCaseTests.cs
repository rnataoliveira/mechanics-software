using FluentAssertions;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Application.Common.Auth;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Application.Features.Auth;
using MechanicsSoftware.Domain.Auth;
using MechanicsSoftware.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MechanicsSoftware.UnitTests.Application.Auth;

public class LoginUseCaseTests
{
    private const string ValidEmail    = "mechanic@example.com";
    private const string ValidPassword = "secret123";
    private const string ValidHash     = "hashed_secret";
    private const string ValidToken    = "jwt.token.here";

    private static User BuildUser(string email = ValidEmail, string hash = ValidHash) =>
        User.Create(Guid.NewGuid(), "Test User", email, hash, User.Roles.Mechanic);

    private static (Mock<IAppDbContext> db, Mock<IPasswordHasher> hasher, Mock<IJwtProvider> jwt)
        BuildDeps(List<User>? users = null)
    {
        var db     = new Mock<IAppDbContext>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt    = new Mock<IJwtProvider>();

        var mockUsers = MockDbSetHelper.CreateMockDbSet(users ?? []);
        db.Setup(d => d.Users).Returns(mockUsers.Object);

        jwt.Setup(j => j.Generate(It.IsAny<User>())).Returns(ValidToken);
        jwt.Setup(j => j.ExpiresAt()).Returns(DateTime.UtcNow.AddHours(1));

        return (db, hasher, jwt);
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsToken()
    {
        var user = BuildUser();
        var (db, hasher, jwt) = BuildDeps(users: [user]);
        hasher.Setup(h => h.Verify(ValidPassword, ValidHash)).Returns(true);

        var useCase = new LoginUseCase(db.Object, hasher.Object, jwt.Object);
        var result  = await useCase.HandleAsync(new LoginRequest(ValidEmail, ValidPassword));

        result.Token.Should().Be(ValidToken);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_EmailNormalized_FindsUserWithUppercaseInput()
    {
        var user = BuildUser();
        var (db, hasher, jwt) = BuildDeps(users: [user]);
        hasher.Setup(h => h.Verify(ValidPassword, ValidHash)).Returns(true);

        var useCase = new LoginUseCase(db.Object, hasher.Object, jwt.Object);
        var result  = await useCase.HandleAsync(new LoginRequest(ValidEmail.ToUpperInvariant(), ValidPassword));

        result.Token.Should().Be(ValidToken);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ThrowsNotFoundException()
    {
        var (db, hasher, jwt) = BuildDeps();

        var useCase = new LoginUseCase(db.Object, hasher.Object, jwt.Object);
        var act     = async () => await useCase.HandleAsync(new LoginRequest(ValidEmail, ValidPassword));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = BuildUser();
        var (db, hasher, jwt) = BuildDeps(users: [user]);
        hasher.Setup(h => h.Verify(ValidPassword, ValidHash)).Returns(false);

        var useCase = new LoginUseCase(db.Object, hasher.Object, jwt.Object);
        var act     = async () => await useCase.HandleAsync(new LoginRequest(ValidEmail, ValidPassword));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_DoesNotGenerateToken()
    {
        var user = BuildUser();
        var (db, hasher, jwt) = BuildDeps(users: [user]);
        hasher.Setup(h => h.Verify(ValidPassword, ValidHash)).Returns(false);

        var useCase = new LoginUseCase(db.Object, hasher.Object, jwt.Object);
        var act     = async () => await useCase.HandleAsync(new LoginRequest(ValidEmail, ValidPassword));

        await act.Should().ThrowAsync<UnauthorizedException>();
        jwt.Verify(j => j.Generate(It.IsAny<User>()), Times.Never);
    }
}
