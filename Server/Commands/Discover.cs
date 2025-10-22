using Frierun.Server.Data;

namespace Frierun.Server;

public class Discover(DiscoverService discoverService)
    : BaseCommand("discover", "Discover and add contracts.")
{
    protected override void Execute()
    {
        discoverService.Discover();
    }
}