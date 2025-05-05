using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ApplicationFactory : Faker<Application>
{
    /// <inheritdoc />
    public ApplicationFactory(Faker<Package> packageFactory, Faker<DockerContainer> containerFactory)
    {
        StrictMode(true);
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.Package, _ => packageFactory.Generate());
        RuleFor(p => p.Url, f => f.Internet.Url());
        RuleFor(p => p.Description, f => f.Lorem.Sentence());
        RuleFor(p => p.Resources, _ => Array.Empty<Resource>());
        RuleFor(p => p.RequiredApplications, _ => Array.Empty<string>());
    }
}