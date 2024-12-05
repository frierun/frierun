using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ApplicationFactory : Faker<Application>
{
    /// <inheritdoc />
    public ApplicationFactory(Faker<Package> packageFactory, Faker<DockerContainer> containerFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.Id, f => f.Random.Guid());
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.Package, _ => packageFactory.Generate());
    }
}