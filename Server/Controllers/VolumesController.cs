using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/volumes")]
public class VolumesController : ControllerBase
{
    [HttpGet]
    public IEnumerable<DockerVolume> List(State state)
    {
        return state.Resources.OfType<DockerVolume>();
    }
}