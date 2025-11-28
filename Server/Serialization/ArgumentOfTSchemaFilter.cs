using Frierun.Server.Data;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frierun.Server;

public class ArgumentOfTSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Argument<>))
        {
            schema.Nullable = true;
            return;
        }
    }
}