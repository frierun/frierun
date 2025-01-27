using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Server.Services;

public class StateSerializer(PackageRegistry packageRegistry)
{
    public string Path { get; } = System.IO.Path.Combine(Storage.DirectoryName, "state.json");

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new PackageConverter(packageRegistry) },
    };

    /// <summary>
    /// Loads state from disk
    /// </summary>
    public State Load()
    {
        if (!File.Exists(Path))
        {
            return new State();
        }
        
        var json = File.ReadAllText(Path);
        return JsonSerializer.Deserialize<State>(json, _serializerOptions) ?? new State();
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