using Bogus;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Tests.Factories;

public sealed class ContainerDefinitionFactor: Faker<ContainerDefinition>
{
    public ContainerDefinitionFactor(Faker<Volume> volumeFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.Command, f => f.Lorem.Sentence());
        RuleFor(p => p.RequireDocker, f => f.Random.Bool());
        RuleFor(p => p.ResourceType, typeof(Container));
    }
}