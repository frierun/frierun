using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record MountContract(
    string Path,
    string VolumeName,
    string? ContainerName = null,
    bool ReadOnly = false
) : Contract<Mount>($"{VolumeName}{(ContainerName == null ? "" : $"in {ContainerName}: ")}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId VolumeId => new(typeof(Volume), VolumeName);
    [JsonIgnore] public ContractId ContainerId => new(typeof(Container), ContainerName);
}