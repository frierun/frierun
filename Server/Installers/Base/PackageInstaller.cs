using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PackageInstaller(State state) : IInstaller<Package>, IUninstaller<Application>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Package>.Initialize(Package package, string _)
    {
        var basePrefix = package.Prefix ?? package.Name;

        var count = 1;
        var prefix = basePrefix;
        while (state.Resources.OfType<Application>().Any(application => application.Name == prefix))
        {
            count++;
            prefix = $"{basePrefix}{count}";
        }

        yield return new InstallerInitializeResult(
            package with
            {
                Prefix = prefix
            },
            null,
            package.Contracts
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Package>.GetDependencies(Package package, ExecutionPlan plan)
    {
        return package.Contracts.Select(contract => new ContractDependency(contract, package));
    }

    /// <inheritdoc />
    Resource IInstaller<Package>.Install(Package package, ExecutionPlan plan)
    {
        var dependencies = plan.GetDependentResources(package).ToList();

        var url = package.ApplicationUrl;
        if (url == null)
        {
            url = dependencies.OfType<GenericHttpEndpoint>().FirstOrDefault()?.Url.ToString();
        }

        if (url == null)
        {
            var portEndpoint = dependencies.OfType<DockerPortEndpoint>().FirstOrDefault();
            if (portEndpoint != null)
            {
                url = $"{portEndpoint.Protocol.ToString().ToLower()}://{portEndpoint.Ip}:{portEndpoint.Port}";
            }
        }

        return new Application(
            Name: package.Prefix!,
            Package: package,
            Url: url,
            Description: package.ApplicationDescription
        );
    }
}