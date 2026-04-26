using System.Text.Json;
using FluentAssertions;
using MechanicsSoftware.API.Middleware;
using MechanicsSoftware.Application.Common.Exceptions;
using MechanicsSoftware.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MechanicsSoftware.UnitTests.API.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;

    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
    }

    private ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ExceptionHandlingMiddleware(next, _mockLogger.Object);
    }

    private HttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private async Task<(int StatusCode, string Body)> InvokeMiddlewareAsync(
        ExceptionHandlingMiddleware middleware,
        HttpContext context)
    {
        await middleware.InvokeAsync(context);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public async Task InvokeAsync_NoException_PassesRequestThrough()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_Returns404()
    {
        // Arrange
        const string notFoundMessage = "Customer with id 'f47ac10b-58cc-4372-a567-0e02b2c3d479' was not found.";
        RequestDelegate next = _ => throw new NotFoundException("Customer", Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"));
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var (statusCode, body) = await InvokeMiddlewareAsync(middleware, context);

        // Assert
        statusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/json");
        var json = JsonDocument.Parse(body);
        var error = json.RootElement.GetProperty("error").GetString();
        error.Should().Contain("Customer");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        RequestDelegate next = _ => throw new UnauthorizedException();
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var (statusCode, body) = await InvokeMiddlewareAsync(middleware, context);

        // Assert
        statusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/json");
        var json = JsonDocument.Parse(body);
        var error = json.RootElement.GetProperty("error").GetString();
        error.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task InvokeAsync_ConflictException_Returns409()
    {
        // Arrange
        var conflictMessage = "Resource already exists";
        RequestDelegate next = _ => throw new ConflictException(conflictMessage);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var (statusCode, body) = await InvokeMiddlewareAsync(middleware, context);

        // Assert
        statusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.ContentType.Should().Be("application/json");
        var json = JsonDocument.Parse(body);
        var error = json.RootElement.GetProperty("error").GetString();
        error.Should().Be(conflictMessage);
    }

    [Fact]
    public async Task InvokeAsync_DomainException_Returns422()
    {
        // Arrange
        var domainMessage = "Invalid domain state";
        RequestDelegate next = _ => throw new DomainException(domainMessage);
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var (statusCode, body) = await InvokeMiddlewareAsync(middleware, context);

        // Assert
        statusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        context.Response.ContentType.Should().Be("application/json");
        var json = JsonDocument.Parse(body);
        var error = json.RootElement.GetProperty("error").GetString();
        error.Should().Be(domainMessage);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500AndLogs()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");
        RequestDelegate next = _ => throw exception;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var (statusCode, body) = await InvokeMiddlewareAsync(middleware, context);

        // Assert
        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
        var json = JsonDocument.Parse(body);
        var error = json.RootElement.GetProperty("error").GetString();
        error.Should().Be("An unexpected error occurred.");
        _mockLogger.Verify(
            x => x.Log(
                Microsoft.Extensions.Logging.LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns500AndLogs()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        RequestDelegate next = _ => throw exception;
        var middleware = CreateMiddleware(next);
        var context = CreateHttpContext();

        // Act
        var (statusCode, body) = await InvokeMiddlewareAsync(middleware, context);

        // Assert
        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var json = JsonDocument.Parse(body);
        var error = json.RootElement.GetProperty("error").GetString();
        error.Should().Be("An unexpected error occurred.");
        _mockLogger.Verify(
            x => x.Log(
                Microsoft.Extensions.Logging.LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
