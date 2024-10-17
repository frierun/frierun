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

    public record ParametersResponse(
        Package Package,
        string Name,
        IEnumerable<List<KeyValuePair<string, string>>> Parameters);

    /// <summary>
    /// Gets package and default parameters
    /// </summary>
    [HttpGet("{id}/parameters")]
    public ParametersResponse? Parameters(string id, PackageRegistry packageRegistry, ExecutionService executionService)
    {
        var package = packageRegistry.Find(id);
        if (package == null)
        {
            return null;
        }

        var executionPlan = executionService.Create(package);
        if (executionPlan == null)
        {
            return null;
        }

        return new ParametersResponse(
            package,
            executionPlan.Name,
            package.Resources
                .Select(resourceDefinition =>
                    executionPlan.Providers[resourceDefinition].GetParameters(executionPlan, resourceDefinition)
                )
                .Select(parameters => parameters.ToList())
        );
    }

    public record InstallRequest(string Name, IEnumerable<List<KeyValuePair<string, string>>> Parameters);

    /// <summary>
    /// Installs the given package
    /// </summary>
    [HttpPost("{id}")]
    public IActionResult Install(
        string id,
        [FromBody] InstallRequest data,
        PackageRegistry packageRegistry,
        InstallService installService,
        ExecutionService executionService
    )
    {
        logger.LogInformation("Installing package {id} with name {name} and port {port}", id, data.Name, 80);

        var package = packageRegistry.Find(id);

        if (package == null)
        {
            return NotFound();
        }

        var executionPlan = executionService.Create(package);
        if (executionPlan == null)
        {
            return Conflict();
        }
        
        executionPlan.Name = data.Name;
        for (var i = 0; i < package.Resources.Count; i++)
        {
            var resourceDefinition = package.Resources[i];
            var parameters = data.Parameters.ElementAt(i);
            executionPlan.Parameters[resourceDefinition] = parameters.ToDictionary();
        }
        
        if (!executionService.Validate(executionPlan))
        {
            return BadRequest();
        }
        
        Task.Run(() => installService.Handle(executionPlan));
        return Accepted();
    }
}