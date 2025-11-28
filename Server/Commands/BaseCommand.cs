using System.CommandLine;

namespace Frierun.Server;

public abstract class BaseCommand : Command
{
    protected BaseCommand(string name, string? description = null) : base(name, description)
    {
        SetAction(_ => Execute());
    }

    protected abstract void Execute();
}