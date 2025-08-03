using System.CommandLine;

namespace Frierun.Server;

public class Run(string[] args, IHostApplicationLifetime applicationLifetime, IEnumerable<Command> commands)
{
    public void Start()
    {
        var rootCommand = new RootCommand();
        foreach (var command in commands)
        {
            rootCommand.Subcommands.Add(command);
        }
        
        var result = rootCommand.Parse(args);
        result.Invoke();
        
        applicationLifetime.StopApplication();
    }
}