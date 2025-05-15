using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class ParameterInstaller : IHandler<Parameter>
{
    public IEnumerable<InstallerInitializeResult> Initialize(Parameter contract, string prefix)
    {
        var value = contract.Value ?? contract.DefaultValue ?? "";

        yield return new InstallerInitializeResult(
            contract with { Value = value, Handler = this }
        );
    }

    public Parameter Install(Parameter contract, ExecutionPlan plan)
    {
        return contract with
        {
            Result = new ResolvedParameter { Name = contract.Name, Value = contract.Value ?? "" }
        };
    }
}