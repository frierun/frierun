using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonConverter(typeof(JsonStringEnumConverter<Protocol>))]
public enum Protocol
{
    Tcp,
    Udp
}