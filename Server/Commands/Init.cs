using System.CommandLine;

namespace Frierun.Server;

public class Init : Command
{
    public Init(ILogger<Init> logger) : base("init", "Initialize the state")
    {
        SetAction(_ => logger.LogInformation("Initializing..."));
    }
}