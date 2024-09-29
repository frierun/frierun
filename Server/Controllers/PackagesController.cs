using Docker.DotNet.Models;
using Frierun.Server.Models;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[ApiController]
[Route("/api/v1/packages")]
public class PackagesController(ILogger<PackagesController> logger) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Package> List(PackageRegistry packageRegistry)
    {
        return packageRegistry.Packages;
    }
    
    [HttpGet("{id}")]
    public Package? Get(string id, PackageRegistry packageRegistry)
    {
        return packageRegistry.Find(id);
    }
    
    public record InstallRequest(string Name, int Port);
    
    [HttpPost("{id}")]
    public IActionResult Install(string id, [FromBody]InstallRequest data, PackageRegistry packageRegistry, InstallService installService)
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