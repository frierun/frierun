﻿using System.Text.Json;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Server.Services;

public class StateSerializer(string path, PackageRegistry packageRegistry)
{
    public string Path { get; } = path;

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