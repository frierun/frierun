using Frierun.Server.Data;

namespace Frierun.Server;

public class DiscoverService(
    ILogger<Discover> logger,
    HandlerRegistry handlerRegistry,
    State state,
    StateSerializer stateSerializer,
    PackageRegistry packageRegistry,
    ExecutionService executionService,
    InstallService installService
    )
{
    public void Discover()
    {
        logger.LogInformation("Discover and add contracts.");

        foreach (var handler in handlerRegistry.GetAllHandlers())
        {
            foreach (var contract in handler.Discover())
            {
                logger.LogInformation("Found contract: {type}", contract);
                state.AddContract(contract);
            }
        }

        stateSerializer.Save(state);
        
        InstallDocker();
    }

    /// <summary>
    /// Auto-install docker package if isn't installed and found on the system.
    /// </summary>
    private void InstallDocker()
    {
        if (state.Applications.Any(app => app.Package?.Name == "docker"))
        {
            return;
        }
        
        var dockerApiConnection = state.GetContracts<DockerApiConnection>().FirstOrDefault();
        if (dockerApiConnection == null)
        {
            return;
        }

        var package = packageRegistry.Find("docker");
        if (package == null)
        {
            logger.LogError("Docker package not found.");
            return;
        }

        logger.LogInformation("Installing docker package.");
        var executionPlan = executionService.Create(package.CreateApplication());
        installService.Handle(executionPlan);
    }
}