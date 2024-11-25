using Frierun.Server.Data;
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

    /// <summary>
    /// Gets package and default parameters
    /// </summary>
    [HttpGet("{id}/plan")]
    public IEnumerable<Contract>? Plan(string id, PackageRegistry packageRegistry, ExecutionService executionService)
    {
        var package = packageRegistry.Find(id);
        if (package == null)
        {
            return null;
        }

        return executionService.Create(package).Contracts.Values;
    }

    /// <summary>
    /// Installs the given package
    /// </summary>
    [HttpPost("{id}/install")]
    public IActionResult Install(
        string id,
        [FromBody] Package overrides,
        PackageRegistry packageRegistry,
        InstallService installService,
        ExecutionService executionService
    )
    {
        var package = packageRegistry.Find(id);

        if (package == null)
        {
            return NotFound();
        }

        var overriden = (Package)package.With(overrides);
        var plan = executionService.Create(overriden);

        logger.LogInformation("Installing package {id}", id);
        Task.Run(() => installService.Handle(plan));
        return Accepted();
    }
}