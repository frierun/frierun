using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PackageInstaller : IInstaller<Package>, IUninstaller<Application>
{
    /// <inheritdoc />
    public IEnumerable<ContractDependency> Dependencies(Package package, ExecutionPlan plan)
    {
        return package.Contracts.Select(contract => new ContractDependency(contract, package));
    }

    /// <inheritdoc />
    public Contract Initialize(Package contract, ExecutionPlan plan)
    {
        var basePrefix = contract.Prefix ?? contract.Name;

        var count = 1;
        var prefix = basePrefix;
        while (plan.State.Resources.OfType<Application>().Any(application => application.Name == prefix))
        {
            count++;
            prefix = $"{basePrefix}{count}";
        }

        return prefix == contract.Prefix
            ? contract
            : contract with
            {
                Prefix = prefix
            };
    }

    /// <inheritdoc />
    public Resource Install(Package package, ExecutionPlan plan)
    {
        var dependencies = package.Contracts
            .Select(contract => plan.GetResource(contract.Id))
            .OfType<Resource>()
            .ToList();
        
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
        )
        {
            DependsOn = dependencies
        };
    }

    /// <inheritdoc />
    public void Uninstall(Application resource)
    {
    }
}