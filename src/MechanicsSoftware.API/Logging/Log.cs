namespace MechanicsSoftware.API.Logging;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger UI: {SwaggerUrl}")]
    internal static partial void SwaggerUIReady(this ILogger logger, string swaggerUrl);
}
