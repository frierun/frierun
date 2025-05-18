using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Frierun.Server;

public class ConfigureJsonOptions(Lazy<HandlerRegistry> lazyHandlerRegistry) : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new LazyHandlerConverter(lazyHandlerRegistry));
    }
}