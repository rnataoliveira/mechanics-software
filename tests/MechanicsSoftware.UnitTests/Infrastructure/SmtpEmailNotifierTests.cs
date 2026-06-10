using FluentAssertions;
using MechanicsSoftware.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;

namespace MechanicsSoftware.UnitTests.Infrastructure;

public class SmtpEmailNotifierTests
{
    private static IConfiguration BuildConfig(
        string? host = "smtp.gmail.com",
        string? port = "587",
        string? user = "user@example.com",
        string? pass = "secret",
        string? from = "noreply@example.com")
    {
        var dict = new Dictionary<string, string?>();
        if (host is not null) dict["SMTP_HOST"] = host;
        if (port is not null) dict["SMTP_PORT"] = port;
        if (user is not null) dict["SMTP_USER"] = user;
        if (pass is not null) dict["SMTP_PASS"] = pass;
        if (from is not null) dict["SMTP_FROM"] = from;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public void Constructor_AllVariablesPresent_DoesNotThrow()
    {
        var act = () => new SmtpEmailNotifier(BuildConfig());

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_MissingHost_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(host: null);

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_HOST*");
    }

    [Fact]
    public void Constructor_MissingPort_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(port: null);

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_PORT*");
    }

    [Fact]
    public void Constructor_MissingUser_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(user: null);

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_USER*");
    }

    [Fact]
    public void Constructor_MissingPass_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(pass: null);

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_PASS*");
    }

    [Fact]
    public void Constructor_MissingFrom_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(from: null);

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_FROM*");
    }

    [Fact]
    public void Constructor_NonNumericPort_ThrowsInvalidOperationExceptionWithDescriptiveMessage()
    {
        var config = BuildConfig(port: "not-a-number");

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_PORT*")
            .WithMessage("*not-a-number*");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("587x")]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_InvalidPortValues_AllThrowInvalidOperationException(string invalidPort)
    {
        var config = BuildConfig(port: invalidPort);

        var act = () => new SmtpEmailNotifier(config);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SMTP_PORT*");
    }
}
