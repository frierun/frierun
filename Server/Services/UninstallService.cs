using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class UninstallService(DockerService dockerService, State state, StateSerializer stateSerializer, StateManager stateManager)
{
    public async Task Handle(Application application)
    {
        if (!stateManager.StartTask("uninstall"))
        {
            return;
        }

        try
        {
            foreach (var container in application.Resources?.OfType<Container>() ?? Enumerable.Empty<Container>())
            {
                await dockerService.StopContainer(container.Name);
            }
            
            foreach (var volume in application.Resources?.OfType<Volume>() ?? Enumerable.Empty<Volume>())
            {
                await dockerService.RemoveVolume(volume.Name);
            }

            state.Applications.Remove(application);
            stateSerializer.Save(state);
        }
        finally
        {
            stateManager.FinishTask();
        }
    }
}