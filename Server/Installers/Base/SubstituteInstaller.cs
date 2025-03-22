using System.Text.RegularExpressions;
using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class SubstituteInstaller(ContractRegistry contractRegistry) : IInstaller<Substitute>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Substitute>.Initialize(Substitute contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract,
            contract.Matches
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
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Substitute>.GetDependencies(Substitute contract, ExecutionPlan plan)
    {
        return contract.Matches
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

                    return new ContractDependency(
                        ContractId.Create(contractType, contractName),
                        contract
                    );
                }
            ).Append(new ContractDependency(contract, contract.OriginalId));
    }

    /// <inheritdoc />
    Resource? IInstaller<Substitute>.Install(Substitute contract, ExecutionPlan plan)
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

        return null;
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
        var resource = plan.GetResource(contractId);
        if (resource == null)
        {
            throw new Exception($"Resource not found: {contractId}");
        }

        var propertyInfo = resource.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            throw new Exception($"Property not found: {propertyName} in {contractType}");
        }

        return propertyInfo.GetValue(resource)?.ToString() ?? "";
    }
}