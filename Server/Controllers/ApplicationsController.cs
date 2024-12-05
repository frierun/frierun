using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/applications")]
public class ApplicationsController : ControllerBase
{
    public record ApplicationResponse(
        Guid Id,
        string Name,
        string? PackageName,
        string? ServiceUrl
    );

    [HttpGet]
    public IEnumerable<ApplicationResponse> List(State state)
    {
        
        return state.Resources.OfType<Application>().Select(
            application =>
            {
                var dependencies = application.AllDependencies.ToList();
                var url = dependencies.OfType<GenericHttpEndpoint>().FirstOrDefault()?.Url;
                if (url == null)
                {
                    var portEndpoint = dependencies.OfType<DockerPortEndpoint>().FirstOrDefault();
                    if (portEndpoint != null)
                    {
                        url = $"{portEndpoint.Protocol.ToString().ToLower()}://{portEndpoint.Ip}:{portEndpoint.Port}";
                    }
                }
                
                return new ApplicationResponse(
                    application.Id,
                    application.Name,
                    application.Package?.Name,
                    url
                );
            }
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(void))]
    public IActionResult Uninstall(Guid id, State state, UninstallService uninstallService)
    {
        var application = state.Resources.OfType<Application>().FirstOrDefault(a => a.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        Task.Run(() => uninstallService.Handle(application));

        return Accepted();
    }
}