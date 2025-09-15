using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class ApplicationHandler : Handler<Application>
{
    public override IEnumerable<ContractList> Initialize(Application application, ApplicationContext context)
    {
        var url = application.Url;

        // auto-detect application URL
        if (url.Empty)
        {
            var httpEndpoint = application.Contracts.Values.OfType<HttpEndpoint>().FirstOrDefault();
            if (httpEndpoint != null)
            {
                url = $"{{{{{httpEndpoint.Id}:Url}}}}";
            }
        }

        // use the first endpoint if not found any other
        if (url.Empty)
        {
            var endpoint = application.Contracts.Values.OfType<PortEndpoint>().FirstOrDefault();
            if (endpoint != null)
            {
                url = $"{{{{{endpoint.Id}:Url}}}}";
            }
        }

        yield return new ContractList(application.Contracts)
        {
            [context] = application with
            {
                Prefix = context.Prefix,
                Url = url,
                Handler = this
            }
        };
    }
}