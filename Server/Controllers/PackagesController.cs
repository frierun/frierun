using Frierun.Server.Data;
using Frierun.Server.Handlers;
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
    [HttpPost("{id}/plan")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(ExecutionPlan))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Package not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Error installing contract", typeof(HandlerExceptionResult))]
    public IActionResult Plan(
        string id,
        [FromBody] Package overrides,
        PackageRegistry packageRegistry,
        ExecutionService executionService
    )
    {
        var package = packageRegistry.Find(id);
        if (package == null)
        {
            return NotFound();
        }

        package = (Package)package.Merge(overrides);
        try
        {
            return Ok(executionService.Create(package));
        }
        catch (HandlerException e)
        {
            return new ConflictObjectResult(e.Result);
        }
    }

    /// <summary>
    /// Installs the given package
    /// </summary>
    [HttpPost("{id}/install")]
    [SwaggerResponse(StatusCodes.Status202Accepted, "Started installation")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Package not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Error installing contract", typeof(HandlerExceptionResult))]
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

        package = (Package)package.Merge(overrides);

        try
        {
            var plan = executionService.Create(package);
            logger.LogInformation("Installing package {id}", id);
            Task.Run(() => installService.Handle(plan));
        }
        catch (HandlerException e)
        {
            return new ConflictObjectResult(e.Result);
        }

        return Accepted();
    }
}