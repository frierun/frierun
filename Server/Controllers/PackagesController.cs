using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(IEnumerable<Contract>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Package not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "No installation options found", typeof(Contract))]
    public IActionResult Plan(string id, PackageRegistry packageRegistry, ExecutionService executionService)
    {
        var package = packageRegistry.Find(id);
        if (package == null)
        {
            return NotFound();
        }

        try
        {
            return Ok(executionService.Create(package).Contracts.Values);
        }
        catch (InstallerNotFoundException e)
        {
            return new ConflictObjectResult(e.Contract)
            {
                DeclaredType = typeof(Contract)
            };
        }
    }

    /// <summary>
    /// Installs the given package
    /// </summary>
    [HttpPost("{id}/install")]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Started installation")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Package not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "No installation options found", typeof(Contract))]
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
        try
        {
            var plan = executionService.Create(overriden);
            logger.LogInformation("Installing package {id}", id);
            Task.Run(() => installService.Handle(plan));
        }
        catch (InstallerNotFoundException e)
        {
            return new ConflictObjectResult(e.Contract)
            {
                DeclaredType = typeof(Contract)
            };
        }

        return Accepted();
    }
}