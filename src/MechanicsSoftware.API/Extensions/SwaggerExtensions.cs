using MechanicsSoftware.API.Filters;
using Microsoft.OpenApi.Models;

namespace MechanicsSoftware.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc( "v1", new OpenApiInfo
            {
                Title = "Mechanics Software API",
                Description = "API for managing auto repair shops.",
                Version = "v1"

            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            options.UseInlineDefinitionsForEnums();
            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }

}
