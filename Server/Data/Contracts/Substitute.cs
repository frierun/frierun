using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Substitute(
    ContractId OriginalId,
    [property: JsonIgnore] IReadOnlyDictionary<string, MatchCollection> Matches
) : Contract(OriginalId.ToString())
{
    public static readonly Regex InsertionRegex = new(@"{{([^}]+)}}", RegexOptions.Compiled);
    public static readonly Regex VariableRegex = new(@"^(\w+):([\w/ ]*):(\w+)$", RegexOptions.Compiled);

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            OriginalId = OnlyOne(OriginalId, contract.OriginalId)
        };
    }
}