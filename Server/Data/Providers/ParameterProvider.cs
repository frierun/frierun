namespace Frierun.Server.Data;

public class ParameterProvider : IInstaller<Parameter>, IUninstaller<ResolvedParameter>
{
    /// <inheritdoc />
    public Contract Initialize(Parameter contract, ExecutionPlan plan)
    {
        var value = contract.Value ?? contract.DefaultValue;
        return (value == contract.Value) 
            ? contract
            : contract with { Value = value };
    }

    /// <inheritdoc />
    public Resource? Install(Parameter contract, ExecutionPlan plan)
    {
        return new ResolvedParameter(contract.Name, contract.Value);
    }

    /// <inheritdoc />
    public void Uninstall(ResolvedParameter resource)
    {
        
    }
}