using System.Diagnostics.CodeAnalysis;
using Frierun.Server.Models;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/packages")]
public class PackagesController(ILogger<PackagesController> logger) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Package> List(PackageRegistry packageRegistry)
    {
        return packageRegistry.Packages;
    }

    public record ParametersResponse(Package Package, [NotNull] string Name, int Port);

    /// <summary>
    /// Gets package and default parameters
    /// </summary>
    [HttpGet("{id}/parameters")]
    public ParametersResponse? Parameters(string id, PackageRegistry packageRegistry, ParameterService parameterService)
    {
        var package = packageRegistry.Find(id);
        if (package == null)
        {
            return null;
        }

        return new ParametersResponse(
            package,
            parameterService.GetDefaultName(package),
            parameterService.GetDefaultPort()
        );
    }

    public record InstallRequest(string Name, int Port);

    /// <summary>
    /// Installs the given package
    /// </summary>
    [HttpPost("{id}")]
    public IActionResult Install(string id, [FromBody] InstallRequest data, PackageRegistry packageRegistry,
        InstallService installService)
    {
        logger.LogInformation("Installing package {id} with name {name} and port {port}", id, data.Name, data.Port);

        var package = packageRegistry.Find(id);

        if (package == null)
        {
            return NotFound();
        }

        _ = installService.Handle(data.Name, data.Port, package);
        return Accepted();
    }
}