using Frierun.Server.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/state")]
public class StateController : ControllerBase
{
    public record State(bool Ready, string? TaskName, HandlerExceptionResult? Error);
    
    [HttpGet]
    [ProducesResponseType(200)]
    public State Get(StateManager stateManager)
    {
        return new State(stateManager.Ready, stateManager.TaskName, stateManager.Error);
    }
}