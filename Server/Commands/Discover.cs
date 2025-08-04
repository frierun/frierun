using Frierun.Server.Data;

namespace Frierun.Server;

public class Discover(ILogger<Discover> logger, HandlerRegistry handlerRegistry, State state, StateSerializer stateSerializer)
    : BaseCommand("discover", "Discover and add contracts.")
{
    protected override void Execute()
    {
        logger.LogInformation("Discover and add contracts.");

        foreach (var handler in handlerRegistry.GetAllHandlers())
        {
            foreach (var contract in handler.Discover())
            {
                logger.LogInformation("Found contract: {type}", contract.Id.Type);
                state.UnmanagedContracts.Add(contract);
            }
        }
        
        stateSerializer.Save(state);
    }
}