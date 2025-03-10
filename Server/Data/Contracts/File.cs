using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record File(
    string Path,
    string? Name = null,
    string? Text = null,
    string? VolumeName = null,
    int? Owner = null,
    int? Group = null
) : Contract(Name ?? $"{Path}{(VolumeName != null ? " in " + VolumeName : "")}")
{
    public string VolumeName { get; } = VolumeName ?? "";
    [JsonIgnore] public ContractId<Volume> VolumeId => new(VolumeName);
}