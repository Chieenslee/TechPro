using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TechPro.API.Services;

/// <summary>
/// Thêm security scheme X-Caller-Email vào OpenAPI document
/// </summary>
public class ApiKeySecurityTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        document.Components.SecuritySchemes["X-Caller-Email"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "X-Caller-Email",
            Description = "Email của người dùng đã đăng nhập trên MVC. Ví dụ: admin@techpro.vn"
        };

        document.SecurityRequirements ??= [];
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "X-Caller-Email"
                    }
                },
                []
            }
        });

        return Task.CompletedTask;
    }
}
