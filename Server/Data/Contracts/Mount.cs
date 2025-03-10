using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Mount(
    string Path,
    string? VolumeName = null,
    string? ContainerName = null,
    bool ReadOnly = false
) : Contract($"{VolumeName}{(ContainerName == null ? "" : $" in {ContainerName}: ")}")
{
    public string VolumeName { get; } = VolumeName ?? "";
    [JsonIgnore] public ContractId<Volume> VolumeId => new(VolumeName);
    
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<Container> ContainerId => new(ContainerName);
}