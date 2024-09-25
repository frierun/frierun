using Frierun.Server.Models;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[ApiController]
[Route("/api/v1/packages")]
public class PackagesController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Package> List(PackageRegistry packageRegistry)
    {
        return packageRegistry.Packages;
    }
    
    [HttpPost("{id}")]
    public IActionResult Install(string id, PackageRegistry packageRegistry, InstallService installService)
    {
        var package = packageRegistry.Find(id);
        
        if (package == null)
        {
            return NotFound();
        }
        
        _ = installService.Handle(package);
        return Accepted();
    }

}