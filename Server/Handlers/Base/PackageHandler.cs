using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PackageHandler : Handler<Package>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Package package, string prefix)
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

        yield return new ContractInitializeResult(
            package with
            {
                Prefix = prefix,
                ApplicationUrl = applicationUrl,
                Handler = this
            },
            package.Contracts
        );
    }

    public override Package Install(Package package, ExecutionPlan plan)
    {
        return package with
        {
            Result = new Application
            {
                Name = package.Prefix!,
                Package = package,
                Url = package.ApplicationUrl,
                Description = package.ApplicationDescription
            }
        };
    }
}