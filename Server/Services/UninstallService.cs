using Frierun.Server.Models;
using Frierun.Server.Services.Serialization;

namespace Frierun.Server.Services;

public class UninstallService(DockerService dockerService, State state, StateManager stateManager)
{
    public async Task Handle(Application application)
    {
        var result = await dockerService.StopContainer(application.Package.Name);
        if (result)
        {
            state.Applications.Remove(application);
            stateManager.Save(state);
        }
    }
}