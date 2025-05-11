using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class ParameterInstaller : IInstaller<Parameter>
{
    public Application? Application => null;

    IEnumerable<InstallerInitializeResult> IInstaller<Parameter>.Initialize(Parameter contract, string prefix)
    {
        var value = contract.Value ?? contract.DefaultValue;

        yield return new InstallerInitializeResult(
            contract with { Value = value }
        );
    }

    Resource IInstaller<Parameter>.Install(Parameter contract, ExecutionPlan plan)
    {
        return new ResolvedParameter(new EmptyHandler()) { Name = contract.Name, Value = contract.Value ?? "" };
    }
}