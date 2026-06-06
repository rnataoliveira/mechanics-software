using Microsoft.Extensions.Logging;

namespace MechanicsSoftware.Infrastructure.Logging;

public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Swagger UI: {SwaggerUrl}")]
    public static partial void SwaggerUIReady(this ILogger logger, string swaggerUrl);
}
