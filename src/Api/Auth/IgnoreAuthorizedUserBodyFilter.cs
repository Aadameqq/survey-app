using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Auth;

public class IgnoreAuthorizedUserBodyFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var bodyParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Source?.Id == "Body" && p.Type != typeof(AuthorizedUser))
            .ToList();

        if (bodyParameters.Count == 0)
        {
            operation.RequestBody = null;
        }
    }
}
