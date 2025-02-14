using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Auth;

public class RemoveAuthorizedUserSchemaFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc.Components.Schemas.ContainsKey(nameof(AuthorizedUser)))
        {
            swaggerDoc.Components.Schemas.Remove(nameof(AuthorizedUser));
        }

        if (swaggerDoc.Components.Schemas.ContainsKey(nameof(AccessManager)))
        {
            swaggerDoc.Components.Schemas.Remove(nameof(AccessManager));
        }
    }
}
