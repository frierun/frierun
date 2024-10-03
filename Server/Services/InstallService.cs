using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class InstallService(DockerService dockerService, State state, StateSerializer stateSerializer)
{
    public async Task Handle(string name, int externalPort, Package package)
    {
        var result = await dockerService.StartContainer(name, externalPort, package);
        if (result)
        {
            var volumeNames = package.Volumes?.Select(volume => $"{name}-{volume.Name}").ToList();
            state.Applications.Add(new Application(Guid.NewGuid(), name, externalPort, volumeNames, package));
            stateSerializer.Save(state);
        }
    }
}
