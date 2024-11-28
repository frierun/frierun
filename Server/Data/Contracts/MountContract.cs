using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record MountContract(
    string Path,
    string VolumeName,
    string? ContainerName = null,
    bool ReadOnly = false
) : Contract($"{VolumeName}{(ContainerName == null ? "" : $"in {ContainerName}: ")}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<VolumeContract> VolumeId => new(VolumeName);
    [JsonIgnore] public ContractId<ContainerContract> ContainerId => new(ContainerName);
}