using System.Text.RegularExpressions;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class SubstituteProvider(ContractRegistry contractRegistry) : IInstaller<Substitute>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(Substitute contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract, contractRegistry.CreateContract(contract.OriginalId));

        var insertions = contract.Matches
            .SelectMany(pair => pair.Value)
            .Select(match => match.Groups[1].Value);
        foreach (var insertion in insertions)
        {
            var match = Substitute.VariableRegex.Match(insertion);
            if (!match.Success)
            {
                throw new Exception($"Invalid insertion format: {insertion}");
            }

            var contractTypeName = match.Groups[1].Value;
            var contractName = match.Groups[2].Value;

            var dependency = contractRegistry.CreateContract(contractTypeName, contractName);
            yield return new ContractDependency(contract, dependency);
        }
    }

    /// <inheritdoc />
    public Resource? Install(Substitute contract, ExecutionPlan plan)
    {
        var original = plan.GetContract(contract.OriginalId);
        if (original is not IHasStrings hasStrings)
        {
            throw new Exception($"Original contract is not IHasStrings: {contract.OriginalId}");
        }

        var changedContract = hasStrings.ApplyStringDecorator(
            s =>
            {
                if (!contract.Matches.TryGetValue(s, out var matchCollection))
                {
                    return s;
                }

                foreach (Match match in matchCollection)
                {
                    var insertion = match.Groups[1].Value;
                    var variableMatch = Substitute.VariableRegex.Match(insertion);
                    if (!variableMatch.Success)
                    {
                        throw new Exception($"Invalid insertion format: {insertion}");
                    }

                    var contractTypeName = variableMatch.Groups[1].Value;
                    var contractName = variableMatch.Groups[2].Value;
                    var propertyName = variableMatch.Groups[3].Value;

                    var dependencyType = contractRegistry.GetContractType(contractTypeName);
                    var dependencyId = new ContractId(dependencyType, contractName);
                    var dependency = plan.GetContract(dependencyId);

                    var propertyInfo = dependencyType.GetProperty(propertyName);
                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property not found: {propertyName} in {dependencyType}");
                    }

                    var propertyValue = propertyInfo.GetValue(dependency)?.ToString() ?? "";
                    s = s.Replace($"{{{{{insertion}}}}}", propertyValue);
                }

                return s;
            }
        );
        plan.UpdateContract(changedContract);

        return null;
    }
}