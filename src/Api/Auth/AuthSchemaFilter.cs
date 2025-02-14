using Core.Domain;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Auth;

public class AuthSchemaFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        RemoveSchemaIfExists(swaggerDoc, nameof(AuthorizedUser));
        RemoveSchemaIfExists(swaggerDoc, nameof(AccessManager));
        RemoveSchemaIfExists(swaggerDoc, nameof(Role));
    }

    private void RemoveSchemaIfExists(OpenApiDocument swaggerDoc, string name)
    {
        if (swaggerDoc.Components.Schemas.ContainsKey(name))
        {
            swaggerDoc.Components.Schemas.Remove(name);
        }
    }
}
