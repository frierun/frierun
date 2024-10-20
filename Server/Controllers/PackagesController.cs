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

    public record ExecutionPlanResponse(
        string TypeName,
        IDictionary<string, string> Parameters,
        IList<ExecutionPlanResponse> Children
    );
    
    public record ParametersResponse(
        Package Package,
        ExecutionPlanResponse ExecutionPlan
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

        var executionPlan = executionService.Create(package);

        return new ParametersResponse(
            package,
            ToResponse(executionPlan)
        );
    }
    
    /// <summary>
    /// Recursively converts an execution plan to a response json
    /// </summary>
    private ExecutionPlanResponse ToResponse(ExecutionPlan executionPlan)
    {
        return new ExecutionPlanResponse(
            executionPlan.Provider.GetType().Name,
            executionPlan.Parameters,
            executionPlan.Children.Select(ToResponse).ToList()
        );
    }
    
    public record ExecutionPlanRequest(
        IDictionary<string, string> Parameters,
        IList<ExecutionPlanRequest> Children
    );
    
    public record InstallRequest(string Name, ExecutionPlanRequest ExecutionPlan);

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
        FromRequest(executionPlan, data.ExecutionPlan);

        if (!executionPlan.Validate())
        {
            return BadRequest();
        }

        Task.Run(() => installService.Handle(executionPlan));
        return Accepted();
    }

    /// <summary>
    /// Recursively fills the execution plan with the given request
    /// </summary>
    private void FromRequest(ExecutionPlan executionPlan, ExecutionPlanRequest request)
    {
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