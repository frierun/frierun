using System.Diagnostics;
using Frierun.Server.Data;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/applications")]
public class ApplicationsController : ControllerBase
{
    [UsedImplicitly]
    public record ApplicationResponse(
        string Name,
        string? PackageName,
        string? Url,
        string? Description,
        string? IconUrl
    );

    [HttpGet]
    public IEnumerable<ApplicationResponse> List(State state)
    {
        return state.Applications.Select(
            application =>
            {
                Debug.Assert(application.Installed);
                return new ApplicationResponse(
                    application.Name,
                    application.Package?.Name,
                    application.Url,
                    application.Description,
                    application.Package?.IconUrl
                );
            }
        );
    }

    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(void))]
    public IActionResult Uninstall(string name, State state, UninstallService uninstallService)
    {
        var application = state.Applications.FirstOrDefault(a => a.Name == name);

        if (application == null)
        {
            return NotFound();
        }

        Task.Run(() => uninstallService.Handle(application));

        return Accepted();
    }
}