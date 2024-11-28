using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record FileContract(
    string Path,
    string Text,
    string? VolumeName = null
) : Contract($"{Path}{(VolumeName != null ? " in " + VolumeName : "")}")
{
    public string VolumeName { get; } = VolumeName ?? "";
    [JsonIgnore] public ContractId<VolumeContract> VolumeId => new(VolumeName);
}