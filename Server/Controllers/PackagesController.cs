using Frierun.Server.Models;
using Frierun.Server.Resources;
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
        ExecutionPlanSelector ExecutionPlan
    );
    
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

        var selector = executionService.Create(package);

        return new ParametersResponse(
            package,
            selector
        );
    }
    
    public record ExecutionPlanRequest(
        int SelectedIndex,
        IDictionary<string, string> Parameters,
        IList<ExecutionPlanRequest> Children
    );
    
    public record InstallRequest(ExecutionPlanRequest ExecutionPlan);

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
        logger.LogInformation("Installing package {id}", id);

        var package = packageRegistry.Find(id);

        if (package == null)
        {
            return NotFound();
        }

        var selector = executionService.Create(package);
        FromRequest(selector, data.ExecutionPlan);

        if (!selector.Selected.Validate())
        {
            return BadRequest();
        }

        Task.Run(() => installService.Handle(selector.Selected));
        return Accepted();
    }

    /// <summary>
    /// Recursively fills the execution plan with the given request
    /// </summary>
    private void FromRequest(ExecutionPlanSelector selector, ExecutionPlanRequest request)
    {
        selector.SelectedIndex = request.SelectedIndex;
        var executionPlan = selector.Selected;
        foreach (var pair in request.Parameters)
        {
            executionPlan.Parameters[pair.Key] = pair.Value;
        }
        for (var i = 0; i < executionPlan.Children.Count; i++)
        {
            FromRequest(executionPlan.Children[i], request.Children[i]);
        }
    }
}