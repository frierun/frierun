using System.Text.Json;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Server;

public class StateSerializer(
    string path,
    PackageRegistry packageRegistry,
    ContractRegistry contractRegistry,
    Lazy<HandlerRegistry> lazyHandlerRegistry
)
{
    public string Path { get; } = path;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new PackageConverter(packageRegistry),
            new ContractIdConverter(contractRegistry),
            new LazyHandlerConverter(lazyHandlerRegistry)
        },
    };

    /// <summary>
    /// Loads state from disk
    /// </summary>
    public State Load()
    {
        if (!File.Exists(Path) || new FileInfo(Path).Length == 0)
        {
            return new State();
        }

        using Stream stream = File.OpenRead(Path);
        return JsonSerializer.Deserialize<State>(stream, _serializerOptions) ?? new State();
    }

    /// <summary>
    /// Saves state to disk
    /// </summary>
    public void Save(State state)
    {
        using Stream stream = File.Open(Path, FileMode.Create, FileAccess.Write);
        JsonSerializer.Serialize(stream, state, _serializerOptions);
    }
}