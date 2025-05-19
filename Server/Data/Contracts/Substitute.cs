using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Frierun.Server.Data;

public record Substitute(
    ContractId OriginalId,
    [property: JsonIgnore] IReadOnlyDictionary<string, MatchCollection> Matches
) : Contract(OriginalId.ToString())
{
    public static readonly Regex InsertionRegex = new(@"{{([^}]+)}}", RegexOptions.Compiled);
    public static readonly Regex VariableRegex = new(@"^(\w+):([\w/ ]*):(\w+)$", RegexOptions.Compiled);
}