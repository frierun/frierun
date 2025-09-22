using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/volumes")]
public class VolumesController : ControllerBase
{
    [HttpGet]
    public IEnumerable<string> List(State state)
    {
        return state
            .GetContracts<Volume>()
            .Select(volume => volume.VolumeName)
            .OfType<string>();
    }
}