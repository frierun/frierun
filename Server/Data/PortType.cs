using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PortType
{
    Tcp,
    Udp
}