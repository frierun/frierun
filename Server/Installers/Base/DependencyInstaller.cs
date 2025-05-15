using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class DependencyInstaller : IHandler<Dependency>
{
    public IEnumerable<InstallerInitializeResult> Initialize(Dependency contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract with
            {
                DependsOn = [contract.Preceding],
                DependencyOf = [contract.Following],
                Handler = this
            }
        );
    }
}