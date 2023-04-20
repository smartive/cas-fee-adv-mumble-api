using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

using MumbleApi.Authentication;

using Swashbuckle.AspNetCore.SwaggerGen;

using Zitadel.Authentication;

namespace MumbleApi.OpenApi;

public class SecurityRequirementFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var zitadelAuth = context.MethodInfo.GetCustomAttribute<ZitadelAuthorizeAttribute>() ??
                          context.MethodInfo.DeclaringType?.GetCustomAttribute<ZitadelAuthorizeAttribute>();
        var optionalAuth = context.MethodInfo.GetCustomAttribute<OptionalZitadelAuthorizeAttribute>() ??
                           context.MethodInfo.DeclaringType?.GetCustomAttribute<OptionalZitadelAuthorizeAttribute>();

        if (zitadelAuth is null && optionalAuth is null)
        {
            return;
        }

        if (optionalAuth is not null)
        {
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "None",
                        },
                    },
                    Array.Empty<string>()
                },
            });
        }

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = ZitadelDefaults.AuthenticationScheme,
                    },
                },
                Array.Empty<string>()
            },
        });

        if (zitadelAuth is not null && optionalAuth is null)
        {
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
        }
    }
}
