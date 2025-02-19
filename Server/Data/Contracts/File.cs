using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record File(
    string Path,
    string Text,
    string? Name = null,
    string? VolumeName = null
) : Contract(Name ?? $"{Path}{(VolumeName != null ? " in " + VolumeName : "")}")
{
    public string VolumeName { get; } = VolumeName ?? "";
    [JsonIgnore] public ContractId<Volume> VolumeId => new(VolumeName);
}