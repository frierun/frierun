using System.Text.RegularExpressions;

namespace Frierun.Server.Data;

public class TemplateResolver : IArgumentResolver<string>
{
    private static readonly Regex InsertionRegex = new(@"{{([^}]+)}}", RegexOptions.Compiled);
    private static readonly Regex VariableRegex = new(@"^(\w+):([\w/ ]*):(\w+)$", RegexOptions.Compiled);
    
    public static TemplateResolver? Create(string value)
    {
        var matchCollection = InsertionRegex.Matches(value);
        if (matchCollection.Count == 0)
        {
            return null;
        }


        return new TemplateResolver(value, matchCollection);
    }

    private TemplateResolver(string value, MatchCollection matchCollection)
    {
        RequiredContracts = matchCollection
            .Select(match => match.Groups[1].Value)
            .Select(insertion =>
                {
                    var match = VariableRegex.Match(insertion);
                    if (!match.Success)
                    {
                        throw new Exception($"Invalid insertion format: {insertion}");
                    }

                    return new ContractId(match.Groups[1].Value, match.Groups[2].Value);
                }
            )
            .ToList();

        _resolver = (plan =>
        {
            foreach (Match match in matchCollection)
            {
                var propertyValue = ResolveInsertion(match.Groups[1].Value, plan);
                value = value.Replace($"{{{{{match.Groups[1].Value}}}}}", propertyValue);
            }

            return value;
        });
    }
    
    private readonly Func<ExecutionPlan, string> _resolver;
    public string? Resolve(ExecutionPlan plan)
    {
        return _resolver.Invoke(plan);
    }

    public IEnumerable<ContractId> RequiredContracts { get; }
    
    /// <summary>
    /// Resolves insertion value.
    /// </summary>
    private static string? ResolveInsertion(string insertion, ExecutionPlan plan)
    {
        var match = VariableRegex.Match(insertion);
        if (!match.Success)
        {
            throw new Exception($"Invalid insertion format: {insertion}");
        }

        var typeName = match.Groups[1].Value;
        var contractName = match.Groups[2].Value;
        var propertyName = match.Groups[3].Value;

        var contractId = new ContractId(typeName, contractName);

        var contract = plan.GetContract(contractId);

        var propertyInfo = contract.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            throw new Exception($"Property not found: {propertyName} in {contractId}");
        }

        if (propertyInfo.PropertyType.IsAssignableTo(typeof(IArgument)))
        {
            var argument = (IArgument)propertyInfo.GetValue(contract)!;
            argument.Resolve(plan);
            return argument.ToString();
        }

        // resolve all contract arguments because our property might depend on any of them
        foreach (var argument in contract.GetArguments())
        {
            argument.Resolve(plan);
        }

        return propertyInfo.GetValue(contract)?.ToString();
    }
}