using Frierun.Server.Models;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/applications")]
public class ApplicationsController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Application> List(State state)
    {
        return state.Applications;
    }
    
    [HttpDelete("{id}")]
    public IActionResult Uninstall(Guid id, State state, UninstallService uninstallService)
    {
        var application = state.Applications.FirstOrDefault(a => a.Id == id);
        
        if (application == null)
        {
            return NotFound();
        }
        
        _ = uninstallService.Handle(application);
        
        return Accepted();
    }
}