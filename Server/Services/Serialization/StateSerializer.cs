using System.Runtime.InteropServices;
using System.Text.Json;
using Frierun.Server.Models;
using Frierun.Server.Resources;
using File = System.IO.File;

namespace Frierun.Server.Services;

public class StateSerializer
{
    public string Path { get; }
    private readonly JsonSerializerOptions _serializerOptions;

    public StateSerializer(PackageRegistry packageRegistry)
    {
        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new PackageConverter(packageRegistry) },
        };

        var localData = Environment.GetEnvironmentVariable(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "LocalAppData"
                : "Home"
        );

        var directory = System.IO.Path.Combine(localData ?? "", "Frierun");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Path = System.IO.Path.Combine(directory, "state.json");
    }

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