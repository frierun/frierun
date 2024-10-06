using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/state")]
public class StateController : ControllerBase
{
    public record State(bool Ready, string? TaskName);
    
    [HttpGet]
    [ProducesResponseType(200)]
    public State Get(StateManager stateManager)
    {
        return new State(stateManager.Ready, stateManager.TaskName);
    }
}