using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Frierun.Server;

public class ConfigureJsonOptions(
    Lazy<HandlerRegistry> lazyHandlerRegistry
)
    : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new ArgumentOfTConverter());
        options.JsonSerializerOptions.Converters.Add(new ContractIdConverter());
        options.JsonSerializerOptions.Converters.Add(new ContractIdOfTConverter());
        options.JsonSerializerOptions.Converters.Add(new ContractListConverter());
        options.JsonSerializerOptions.Converters.Add(new LazyHandlerConverter(lazyHandlerRegistry));
    }
}