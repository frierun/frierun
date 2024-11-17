using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class ContainerDefinitionFactor: Faker<ContainerContract>
{
    public ContainerDefinitionFactor(Faker<Volume> volumeFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.RequireDocker, f => f.Random.Bool());
        RuleFor(p => p.ResourceType, typeof(Container));
    }
}