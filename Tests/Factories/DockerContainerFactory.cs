using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class DockerContainerFactory : Faker<DockerContainer>
{
    /// <inheritdoc />
    public DockerContainerFactory()
    {
        StrictMode(true);
        this.SkipConstructor();
        //RuleFor(p => p.Id, f => f.Random.Guid());
        RuleFor(p => p.Name, f => f.Lorem.Word());
    }
}