using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Application), nameof(Application))]
[JsonDerivedType(typeof(DockerContainer), nameof(DockerContainer))]
[JsonDerivedType(typeof(DockerNetwork), nameof(DockerNetwork))]
[JsonDerivedType(typeof(DockerPortEndpoint), nameof(DockerPortEndpoint))]
[JsonDerivedType(typeof(DockerVolume), nameof(DockerVolume))]
[JsonDerivedType(typeof(GenericHttpEndpoint), nameof(GenericHttpEndpoint))]
[JsonDerivedType(typeof(GeneratedPassword), nameof(GeneratedPassword))]
[JsonDerivedType(typeof(LocalPath), nameof(LocalPath))]
[JsonDerivedType(typeof(MysqlDatabase), nameof(MysqlDatabase))]
[JsonDerivedType(typeof(PostgresqlDatabase), nameof(PostgresqlDatabase))]
[JsonDerivedType(typeof(RedisDatabase), nameof(RedisDatabase))]
[JsonDerivedType(typeof(ResolvedDomain), nameof(ResolvedDomain))]
[JsonDerivedType(typeof(ResolvedParameter), nameof(ResolvedParameter))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
public abstract class Resource(Lazy<IHandler> lazyHandler)
{
    protected Resource(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }

    [JsonPropertyName("Handler")]
    public Lazy<IHandler> LazyHandler => lazyHandler;
    
    [JsonIgnore]
    public virtual IHandler Handler => LazyHandler.Value;
    
    public void Uninstall()
    {
        Handler.Uninstall(this);
    }
}
