using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record File(
    string? Name = null,
    string? Path = null,
    string? Text = null,
    string? VolumeName = null
) : Contract(Name ?? $"{Path}{(VolumeName != null ? " in " + VolumeName : "")}")
{
    public string Text { get; init; } = Text ?? "";
    public string VolumeName { get; } = VolumeName ?? "";
    [JsonIgnore] public ContractId<Volume> VolumeId => new(VolumeName);
}