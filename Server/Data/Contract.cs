using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public abstract record Contract<T>(
    string Name
) : Contract(typeof(T), Name)
    where T : Resource;

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
    [property:JsonIgnore]Type ResourceType,
    string Name,
    [property:JsonIgnore]Provider? Provider = null
)
{
    [JsonIgnore] public ContractId Id => new(ResourceType, Name);
    public string? ProviderType => Provider?.GetType().Name;

    public virtual Contract With(Contract other)
    {
        throw new Exception("Not implemented");
    }
}