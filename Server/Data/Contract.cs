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
    [property:JsonIgnore]IInstaller? Installer = null
)
{
    [JsonIgnore] public ContractId Id => new(GetType(), Name);
    public string? InstallerType => Installer?.GetType().Name;

    public virtual Contract With(Contract other)
    {
        throw new Exception("Not implemented");
    }
}