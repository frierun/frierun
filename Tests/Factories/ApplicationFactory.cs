using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Installers;

namespace Frierun.Tests.Factories;

public sealed class ApplicationFactory : Faker<Application>
{
    public ApplicationFactory(Faker<Package> packageFactory)
    {
        RuleFor(p => p.Package, _ => packageFactory.Generate());
        RuleFor(p => p.Url, f => f.Internet.Url());
        RuleFor(p => p.Description, f => f.Lorem.Sentence());
        RuleFor(p => p.Contracts, _ => Array.Empty<Contract>());
        RuleFor(p => p.RequiredApplications, _ => Array.Empty<string>());
    }
}