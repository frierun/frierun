using System.Runtime.InteropServices;
using Frierun.Server.Models;
using Newtonsoft.Json;

namespace Frierun.Server.Services;

public class StateSerializer
{
    public string Path { get; }
    private readonly JsonSerializer _serializer;

    public StateSerializer(PackageRegistry packageRegistry)
    {
        _serializer = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new ContractResolver(packageRegistry)
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

        using StreamReader file = File.OpenText(Path);
        JsonReader reader = new JsonTextReader(file);
        return _serializer.Deserialize<State>(reader) ?? new State();
    }

    /// <summary>
    /// Saves state to disk
    /// </summary>
    public void Save(State state)
    {
        using StreamWriter file = File.CreateText(Path);
        _serializer.Serialize(file, state);
    }
}