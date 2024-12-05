using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class ContainerDefinitionFactor: Faker<Container>
{
    public ContainerDefinitionFactor(Faker<DockerVolume> volumeFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.RequireDocker, f => f.Random.Bool());
    }
}