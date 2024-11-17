using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record ContractId(
    [property: JsonIgnore] Type Type,
    string Name
);