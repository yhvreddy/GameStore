using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.RegularExpressions;

namespace GameStore.Swagger;

public class AuthorizeDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var apiDescription in context.ApiDescriptions)
        {
            var metadata = apiDescription.ActionDescriptor.EndpointMetadata;
            bool allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
            bool requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

            if (!requiresAuthorization || allowsAnonymous)
            {
                continue;
            }

            string path = "/" + apiDescription.RelativePath?.TrimEnd('/');
            path = Regex.Replace(path, @"\{([^}:]+):[^}]+\}", "{$1}");
            if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem))
            {
                continue;
            }

            var method = new HttpMethod(apiDescription.HttpMethod!);
            if (pathItem.Operations is null || !pathItem.Operations.TryGetValue(method, out var operation))
            {
                continue;
            }

            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", swaggerDoc, null),
                        []
                    }
                }
            ];
        }
    }
}
