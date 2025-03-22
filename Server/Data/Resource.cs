using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Application), nameof(Application))]
[JsonDerivedType(typeof(DockerAttachedNetwork), nameof(DockerAttachedNetwork))]
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
[JsonDerivedType(typeof(ResolvedParameter), nameof(ResolvedParameter))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
public abstract record Resource
{
}