using Frierun.Server.Installers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frierun.Server;

public class LazyHandlerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type != typeof(IHandler) && type != typeof(Lazy<IHandler>))
        {
            return;
        }

        schema.Type = "object";
        schema.Properties = new Dictionary<string, OpenApiSchema>
        {
            ["TypeName"] = new()
            {
                Type = "string",
            },
            ["ApplicationName"] = new()
            {
                Type = "string",
                Nullable = true
            }
        };
        schema.Required = new HashSet<string>
        {
            "TypeName"
        };
        
        schema.AdditionalPropertiesAllowed = false;
        schema.Example = new OpenApiObject
        {
            ["TypeName"] = new OpenApiString(nameof(TraefikHttpEndpointInstaller)),
            ["ApplicationName"] = new OpenApiString("Traefik"),
        };
    }
}