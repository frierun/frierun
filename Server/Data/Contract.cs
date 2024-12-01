using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ContainerContract), "Container")]
[JsonDerivedType(typeof(FileContract), "File")]
[JsonDerivedType(typeof(HttpEndpointContract), "HttpEndpoint")]
[JsonDerivedType(typeof(MountContract), "Mount")]
[JsonDerivedType(typeof(NetworkContract), "Network")]
[JsonDerivedType(typeof(Package), "Application")]
[JsonDerivedType(typeof(PortEndpointContract), "PortEndpoint")]
[JsonDerivedType(typeof(VolumeContract), "Volume")]
public abstract record Contract(
    string Name,
    IInstaller? Installer = null,
    IEnumerable<Resource>? DependsOn = null
)
{
    [JsonIgnore] public ContractId Id => new(GetType(), Name);
    [JsonIgnore] public IInstaller? Installer { get; init; } = Installer;
    public string? InstallerType => Installer?.GetType().Name;
    
    [JsonIgnore]
    public IEnumerable<Resource> DependsOn { get; init; } = DependsOn ?? Array.Empty<Resource>();


    public virtual Contract With(Contract other)
    {
        throw new Exception("Not implemented");
    }
}