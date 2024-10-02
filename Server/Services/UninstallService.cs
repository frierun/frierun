using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class UninstallService(DockerService dockerService, State state, StateManager stateManager)
{
    public async Task Handle(Application application)
    {
        var result = await dockerService.StopContainer(application.Name);
        if (application.Package is { Volumes: not null })
        {
            foreach (var volumeName in application.VolumeNames ?? Enumerable.Empty<string>())
            {
                await dockerService.RemoveVolume(volumeName);
            }
        }
        
        if (result)
        {
            state.Applications.Remove(application);
            stateManager.Save(state);
        }
    }
}