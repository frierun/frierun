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
            foreach (var container in application.AllResources.OfType<Container>())
            {
                await dockerService.StopContainer(container.Name);
            }
            
            foreach (var volume in application.AllResources.OfType<Volume>())
            {
                await dockerService.RemoveVolume(volume.Name);
            }

            foreach (var containerGroup in application.AllResources.OfType<ContainerGroup>())
            {
                await dockerService.RemoveNetwork(containerGroup.Name);
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