namespace Frierun.Server;

public class Discover(DiscoveryService discoveryService)
    : BaseCommand("discover", "Discover and add contracts.")
{
    protected override void Execute()
    {
        discoveryService.Discover();
    }
}