using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class DaemonFactory : ContractFaker<Daemon>
{
    public DaemonFactory()
    {
        CustomInstantiator(_ => new Daemon());
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