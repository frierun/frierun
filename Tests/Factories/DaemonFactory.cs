using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class DaemonFactory : Faker<Daemon>
{
    private readonly HashSet<string?> _uniqueNames = [];

    public DaemonFactory()
    {
        CustomInstantiator(_ => new Daemon(""));
        this.UniqueRuleFor(p => p.Name, f => f.Lorem.Word(), _uniqueNames);
        RuleFor(p => p.Command, f => new Argument<IEnumerable<string>>(new List<string>(f.Lorem.Words())));
        RuleFor(
            p => p.PreCommands,
            new Func<Faker, object>(f =>
                new Argument<IEnumerable<IEnumerable<string>>>(
                    f.Make(f.Random.Number(3), () => new List<string>(f.Lorem.Words()))
                )
            )
        );
    }
}