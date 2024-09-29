using Bogus;
using Frierun.Server.Models;

namespace Frierun.Tests.Factories;

public sealed class ApplicationFactory : Faker<Application>
{
    /// <inheritdoc />
    public ApplicationFactory(Faker<Package> packageFactory)
    {
        StrictMode(true);
        this.SkipConstructor();
        RuleFor(p => p.Id, f => f.Random.Guid());
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.Port, f => f.Random.Number(1, 65535));
        RuleFor(p => p.Package, _ => packageFactory.Generate());
    }
}