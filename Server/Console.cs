using System.CommandLine;

namespace Frierun.Server;

public class Console(IEnumerable<Command> commands)
{
    public int Run(string[] args)
    {
        var rootCommand = new RootCommand();
        foreach (var command in commands)
        {
            rootCommand.Subcommands.Add(command);
        }
        
        return rootCommand.Parse(args).Invoke();
    }
}