using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PackageInstaller : IInstaller<Package>, IUninstaller<Application>
{
    /// <inheritdoc />
    public Application? Application => null;
    
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Package>.Initialize(Package package, string prefix)
    {
        var applicationUrl = package.ApplicationUrl;
        
        // auto-detect application URL
        if (applicationUrl == null)
        {
            var httpEndpoint = package.Contracts.OfType<HttpEndpoint>().FirstOrDefault();
            if (httpEndpoint != null)
            {
                applicationUrl = $"{{{{{httpEndpoint.Id}:Url}}}}";
            }
        }
        
        // use the first endpoint if not found any other
        if (applicationUrl == null)
        {
            var endpoint = package.Contracts.OfType<PortEndpoint>().FirstOrDefault();
            if (endpoint != null)
            {
                applicationUrl = $"{{{{{endpoint.Id}:Url}}}}";
            }
        }
        
        yield return new InstallerInitializeResult(
            package with
            {
                Prefix = prefix,
                ApplicationUrl = applicationUrl,
            },
            package.Contracts
        );
    }

    /// <inheritdoc />
    Resource IInstaller<Package>.Install(Package package, ExecutionPlan plan)
    {
        return new Application(
            Name: package.Prefix!,
            Package: package,
            Url: package.ApplicationUrl,
            Description: package.ApplicationDescription
        );
    }
}