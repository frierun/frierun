using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Mount(
    string Path,
    string VolumeName,
    string? ContainerName = null,
    bool ReadOnly = false
) : Contract($"{VolumeName}{(ContainerName == null ? "" : $"in {ContainerName}: ")}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<Volume> VolumeId => new(VolumeName);
    [JsonIgnore] public ContractId<Container> ContainerId => new(ContainerName);
}