using System.Reflection;
using System.Runtime.InteropServices;
using Frierun.Server.Models;
using Newtonsoft.Json;

namespace Frierun.Server.Services;

public class StateManager
{
    private readonly string _path;

    public StateManager()
    {
        var localData = Environment.GetEnvironmentVariable(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "LocalAppData"
                : "Home"
        );
        
        var directory = Path.Combine(localData ?? "", "Frierun");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _path = Path.Combine(directory, "state.json");
    }

    public State Load()
    {
        if (!File.Exists(_path))
        {
            return new State();
        }

        return JsonConvert.DeserializeObject<State>(File.ReadAllText(_path)) ?? new State();
    }

    public void Save(State state)
    {
        File.WriteAllText(_path, JsonConvert.SerializeObject(state, Formatting.Indented));
    }
}