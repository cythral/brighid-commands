using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Fixes the Commands:ExecuteCommand endpoint definition to accept any object as a body.
    /// </summary>
    public class ExecuteCommandFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor && descriptor.ControllerName == "Command" && descriptor.ActionName == "Execute")
            {
                context.SchemaRepository.AddDefinition("ExecuteCommandRequest", new OpenApiSchema
                {
                    Title = "ExecuteCommandRequest",
                    Type = "object",
                    AdditionalPropertiesAllowed = true,
                });

                operation.Parameters.RemoveAt(1); // remove header value parameters
                operation.Parameters.RemoveAt(1);

                operation.RequestBody = new OpenApiRequestBody()
                {
                    Content = new Dictionary<string, OpenApiMediaType>()
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "ExecuteCommandRequest",
                                },
                            },
                        },
                    },
                };
            }
        }
    }
}
