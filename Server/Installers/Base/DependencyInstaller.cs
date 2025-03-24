﻿using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class DependencyInstaller : IInstaller<Dependency>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Dependency>.Initialize(Dependency contract, string prefix)
    {
        yield return new InstallerInitializeResult(
            contract with
            {
                DependsOn = [contract.Preceding],
                DependencyOf = [contract.Following]
            }
        );
    }
}