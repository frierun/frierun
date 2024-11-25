using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Protocol
{
    Tcp,
    Udp
}