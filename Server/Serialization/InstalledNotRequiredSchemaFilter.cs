using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frierun.Server;

public class InstalledNotRequiredSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        schema.AllOf.Append(schema).ToList().ForEach(
            apiSchema =>
            {
                apiSchema.Properties
                    .Where(p => p.Value.Type == "boolean" && p.Key == "installed")
                    .ToList()
                    .ForEach(p => { apiSchema.Required.Remove(p.Key); });
            }
        );
    }
}