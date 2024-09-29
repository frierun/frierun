using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class InstallService(DockerService dockerService, State state, StateManager stateManager)
{
    public async Task Handle(string name, int externalPort, Package package)
    {
        var result = await dockerService.StartContainer(name, package.ImageName, externalPort, package.Port);
        if (result)
        {
            state.Applications.Add(new Application(Guid.NewGuid(), name, externalPort, package));
            stateManager.Save(state);
        }
    }
}
