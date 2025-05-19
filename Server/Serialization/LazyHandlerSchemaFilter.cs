using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frierun.Server;

public class LazyHandlerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type == typeof(Contract))
        {
            // Handler can be nullable
            schema.Required.Remove("handler");
            return;
        }
        
        if (type != typeof(IHandler) && type != typeof(Lazy<IHandler>) && type != typeof(Lazy<IHandler?>))
        {
            return;
        }

        schema.Type = "object";
        schema.Properties = new Dictionary<string, OpenApiSchema>
        {
            ["typeName"] = new()
            {
                Type = "string",
            },
            ["applicationName"] = new()
            {
                Type = "string",
                Nullable = true
            }
        };
        schema.Required = new HashSet<string>
        {
            "typeName"
        };
        
        schema.AdditionalPropertiesAllowed = false;
        schema.Example = new OpenApiObject
        {
            ["typeName"] = new OpenApiString(nameof(TraefikHttpEndpointHandler)),
            ["applicationName"] = new OpenApiString("Traefik"),
        };
    }
}