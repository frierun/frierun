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

                    var contractRef = new ContractRef(match.Groups[1].Value, match.Groups[2].Value); 
                    return new ContractId(Guid.Empty, contractRef);
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
    public string Resolve(ExecutionPlan plan)
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

        var contractRef = new ContractRef(typeName, contractName);

        var contract = plan.GetContract(contractRef);

        var propertyInfo = contract.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            throw new Exception($"Property not found: {propertyName} in {contractRef}");
        }

        if (!propertyInfo.PropertyType.IsAssignableTo(typeof(IArgument)))
        {
            return propertyInfo.GetValue(contract)?.ToString();
        }

        var argument = (IArgument)propertyInfo.GetValue(contract)!;
        return argument.Resolve(plan).ToString();
    }
}