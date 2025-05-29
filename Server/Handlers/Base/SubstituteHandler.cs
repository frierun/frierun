using System.Text.RegularExpressions;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class SubstituteHandler(ContractRegistry contractRegistry) : Handler<Substitute>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Substitute contract, string prefix)
    {
        var contractIds = contract.Matches
            .SelectMany(pair => pair.Value)
            .Select(match => match.Groups[1].Value)
            .Select(
                insertion =>
                {
                    var match = Substitute.VariableRegex.Match(insertion);
                    if (!match.Success)
                    {
                        throw new Exception($"Invalid insertion format: {insertion}");
                    }

                    var contractTypeName = match.Groups[1].Value;
                    var contractType = contractRegistry.GetContractType(contractTypeName);
                    var contractName = match.Groups[2].Value;

                    return ContractId.Create(contractType, contractName);
                }
            )
            .ToList();
        
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DependsOn = contract.DependsOn.Concat(contractIds),
                DependencyOf = contract.DependencyOf.Append(contract.OriginalId)
            }
        );
    }

    public override Substitute Install(Substitute contract, ExecutionPlan plan)
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
                    var propertyValue = ResolveInsertion(match.Groups[1].Value, plan);
                    s = s.Replace($"{{{{{match.Groups[1].Value}}}}}", propertyValue);
                }

                return s;
            }
        );
        plan.UpdateContract(changedContract);

        return contract;
    }

    /// <summary>
    /// Resolves insertion value.
    /// </summary>
    private string ResolveInsertion(string insertion, ExecutionPlan plan)
    {
        var match = Substitute.VariableRegex.Match(insertion);
        if (!match.Success)
        {
            throw new Exception($"Invalid insertion format: {insertion}");
        }

        var contractTypeName = match.Groups[1].Value;
        var contractName = match.Groups[2].Value;
        var propertyName = match.Groups[3].Value;

        var contractType = contractRegistry.GetContractType(contractTypeName);
        var contractId = ContractId.Create(contractType, contractName);
        
        object result;
        if (contractType.IsAssignableTo(typeof(IHasResult)))
        {
            result = plan.GetResource(contractId);
        }
        else
        {
            result = plan.GetContract(contractId);
        }

        var propertyInfo = result.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            throw new Exception($"Property not found: {propertyName} in {contractType}");
        }

        return propertyInfo.GetValue(result)?.ToString() ?? "";
    }
}