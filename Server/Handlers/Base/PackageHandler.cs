using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PackageHandler : Handler<Package>
{
    public override IEnumerable<ContractList> Initialize(Package package, ApplicationContext context)
    {
        var applicationUrl = package.ApplicationUrl;

        // auto-detect application URL
        if (applicationUrl.Empty)
        {
            var httpEndpoint = package.Contracts.Values.OfType<HttpEndpoint>().FirstOrDefault();
            if (httpEndpoint != null)
            {
                applicationUrl = $"{{{{{httpEndpoint.Id}:Url}}}}";
            }
        }

        // use the first endpoint if not found any other
        if (applicationUrl.Empty)
        {
            var endpoint = package.Contracts.Values.OfType<PortEndpoint>().FirstOrDefault();
            if (endpoint != null)
            {
                applicationUrl = $"{{{{{endpoint.Id}:Url}}}}";
            }
        }

        yield return [
            package with
            {
                Prefix = context.Prefix,
                ApplicationUrl = applicationUrl,
                Handler = this
            },
            ..package.Contracts
        ];
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