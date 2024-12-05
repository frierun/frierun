using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ContainerFactory : Faker<DockerContainer>
{
    /// <inheritdoc />
    public ContainerFactory(Faker<DockerVolume> volumeFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        //RuleFor(p => p.Id, f => f.Random.Guid());
        RuleFor(p => p.Name, f => f.Lorem.Word());
    }
}