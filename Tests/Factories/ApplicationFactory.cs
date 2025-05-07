using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Installers;

namespace Frierun.Tests.Factories;

public sealed class ApplicationFactory : Faker<Application>
{
    /// <inheritdoc />
    public ApplicationFactory(Faker<Package> packageFactory)
    {
        StrictMode(true);
        CustomInstantiator(f => new Application(NSubstitute.Substitute.For<IHandler>())
            {
                Name = f.Lorem.Word()
            }
        );
        RuleFor(p => p.Package, _ => packageFactory.Generate());
        RuleFor(p => p.Url, f => f.Internet.Url());
        RuleFor(p => p.Description, f => f.Lorem.Sentence());
        RuleFor(p => p.Resources, _ => Array.Empty<Resource>());
        RuleFor(p => p.RequiredApplications, _ => Array.Empty<string>());
        Ignore(p => p.Name);
    }
}