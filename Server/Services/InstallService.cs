using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class InstallService(DockerService dockerService, State state, StateManager stateManager)
{
    public async Task Handle(Package package)
    {
        var result = await dockerService.StartContainer(package.Name, package.ImageName, package.Port);
        if (result)
        {
            state.Applications.Add(new Application(Guid.NewGuid(), package));
            stateManager.Save(state);
        }
    }
}
