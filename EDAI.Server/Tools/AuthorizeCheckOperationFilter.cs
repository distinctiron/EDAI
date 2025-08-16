using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EDAI.Server.Tools;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation openApiOperation, OperationFilterContext operationFilterContext)
    {
        var hasAuthorize = operationFilterContext.MethodInfo.DeclaringType
                               .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                           operationFilterContext.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>()
                               .Any();

        if (hasAuthorize)
        {
            openApiOperation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                    
                }

            };
        }
    }
    
}