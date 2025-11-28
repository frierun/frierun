using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/contracts")]
public class ContractsController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Contract> List(State state)
    {
        return state.Contracts.Values;
    }

    /// <summary>
    /// Discovers contracts.
    /// </summary>
    [HttpPost("discover")]
    public IActionResult Discover(DiscoveryService discoveryService)
    {
        discoveryService.Discover();
        return Ok();
    }
}