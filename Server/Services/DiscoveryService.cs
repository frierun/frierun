using System.Diagnostics;
using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Server;

public class DiscoveryService(
    ILogger<Discover> logger,
    HandlerRegistry handlerRegistry,
    State state,
    StateSerializer stateSerializer,
    PackageRegistry packageRegistry,
    ExecutionService executionService,
    InstallService installService,
    StateManager stateManager
)
{
    public void Discover()
    {
        logger.LogInformation("Discover and add contracts.");

        if (!stateManager.StartTask("discover"))
        {
            return;
        }

        try
        {
            foreach (var handler in handlerRegistry.GetAllHandlers())
            {
                Discover(handler);
            }
            stateSerializer.Save(state);
        }
        finally
        {
            stateManager.FinishTask();
        }

        InstallDocker();
    }

    /// <summary>
    /// Discovers contracts for the application.
    /// </summary>
    public void Discover(Application application)
    {
        foreach (var handler in handlerRegistry.GetHandlers(application))
        {
            Discover(handler);
        }
    }

    /// <summary>
    /// Discovers contracts for the handler.
    /// </summary>
    private void Discover(IHandler handler)
    {
        Debug.Assert(!stateManager.Ready, "Task must be already running.");
        
        foreach (var contract in handler.Discover())
        {
            if (state.Contracts.Values
                .Where(installedContract => installedContract.Handler == handler)
                .Any(installedContract => installedContract.IsSubset(contract))
               )
            {
                continue;
            }

            AddContract(
                contract with
                {
                    Id = Guid.CreateVersion7(),
                    Handler = handler
                }
            );
        }
    }

    /// <summary>
    /// Adds contract to the state.
    /// </summary>
    private void AddContract(Contract contract)
    {
        logger.LogInformation("Found contract: {type}", contract);
        state.AddContract(contract);
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