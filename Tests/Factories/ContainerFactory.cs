using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ContainerFactory : Faker<Container>
{
    /// <inheritdoc />
    public ContainerFactory(Faker<Volume> volumeFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        //RuleFor(p => p.Id, f => f.Random.Guid());
        RuleFor(p => p.Name, f => f.Lorem.Word());
    }
}