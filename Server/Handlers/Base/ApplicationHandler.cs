using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class ApplicationHandler(State state) : Handler<Application>(state)
{
    public override IEnumerable<ContractList> Initialize(Application application, ApplicationContext context)
    {
        Debug.Assert(application.Name == context.Prefix);
        
        var url = application.Url;

        // auto-detect application URL
        if (url.Empty)
        {
            var httpEndpointId = application.Package?.Contracts
                .Where(pair => pair.Value.GetType() == typeof(HttpEndpoint))
                .Select(pair => pair.Key)
                .FirstOrDefault();
            if (httpEndpointId != null)
            {
                url = $"{{{{{httpEndpointId}:Url}}}}";
            }
        }

        // use the first endpoint if not found any other
        if (url.Empty)
        {
            var endpointId = application.Package?.Contracts
                .Where(pair => pair.Value.GetType() == typeof(PortEndpoint))
                .Select(pair => pair.Key)
                .FirstOrDefault();
            
            if (endpointId != null)
            {
                url = $"{{{{{endpointId}:Url}}}}";
            }
        }

        yield return new ContractList(application.Package?.Contracts ?? [])
        {
            [context] = application with
            {
                Url = url,
                Handler = this
            }
        };
    }
}