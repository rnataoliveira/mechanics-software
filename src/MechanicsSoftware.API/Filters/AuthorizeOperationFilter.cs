using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MechanicsSoftware.API.Filters;

[ExcludeFromCodeCoverage]
public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any()
            ||
            context.MethodInfo.DeclaringType!
            .GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (hasAllowAnonymous)
        {
            operation.Security.Clear();
        }
    }
}
