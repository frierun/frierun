namespace Frierun.Server.Data;

public class ParameterProvider : IInstaller<Parameter>
{
    /// <inheritdoc />
    public Contract Initialize(Parameter contract, ExecutionPlan plan)
    {
        var value = contract.Value ?? contract.DefaultValue;
        return (value == contract.Value) 
            ? contract
            : contract with { Value = value };
    }
}