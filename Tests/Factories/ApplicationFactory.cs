using Bogus;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ApplicationFactory : ContractFaker<Application>
{
    public ApplicationFactory(Faker<Package> packageFactory)
    {
        CustomInstantiator(_ => new Application(""));
        RuleFor(p => p.Package, _ => packageFactory.Generate());
        RuleFor(p => p.Url, f => new Argument<string>(f.Internet.Url()));
        RuleFor(p => p.Description, f => new Argument<string>(f.Lorem.Sentence()));
    }
}