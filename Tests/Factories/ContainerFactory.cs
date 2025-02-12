using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ContainerFactory: Faker<Container>
{
    public ContainerFactory()
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.RequireDocker, f => f.Random.Bool());
    }
}