using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace MumbleApi.OpenApi;

public class InternalServerErrorResponseFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.Add("500", new()
        {
            Description = "Internal Server Error",
        });
    }
}
