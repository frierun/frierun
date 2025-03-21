﻿using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Frierun.Server.Data;

public record Substitute(
    ContractId OriginalId,
    IReadOnlyDictionary<string, MatchCollection> Matches
) : Contract(OriginalId.ToString())
{
    public static readonly Regex InsertionRegex = new Regex(@"{{([^}]+)}}", RegexOptions.Compiled);
    public static readonly Regex VariableRegex = new Regex(@"^(\w+):(\w*):(\w+)$", RegexOptions.Compiled);
    
    [JsonIgnore] public IReadOnlyDictionary<string, MatchCollection> Matches { get; init; } = Matches;
}