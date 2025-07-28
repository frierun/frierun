using Bogus;
using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class DaemonFactory : Faker<Daemon>
{
    public DaemonFactory()
    {
        CustomInstantiator(_ => new Daemon(""));
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.Command, f => new List<string>(f.Lorem.Words()));
        RuleFor(
            p => p.PreCommands,
            new Func<Faker, object>(f => f.Make(f.Random.Number(3), () => new List<string>(f.Lorem.Words())))
        );
    }
}