using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Brighid.Commands.Service
{
    /// <summary>
    /// Adds a display name field to the Swagger.
    /// </summary>
    public class DisplayNameFilter : ISchemaFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var propertyNameAttribute = context.MemberInfo?.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (propertyNameAttribute != null)
            {
                schema.AddExtension("x-display-name", new DisplayNameExtension(context.MemberInfo!.Name));
            }
        }
    }
}
