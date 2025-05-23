using Bogus;
using Docker.DotNet.Models;
using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ContainerFactory: Faker<Container>
{
    public ContainerFactory()
    {
        CustomInstantiator(_ => new Container(""));
        RuleFor(p => p.Name, f => f.Lorem.Word());
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        RuleFor(p => p.ImageName, f => f.Internet.Url());
        RuleFor(p => p.RequireDocker, f => f.Random.Bool());
        RuleFor(p => p.NetworkName, _ => "");
        RuleFor(p => p.Command, f => new List<string>(f.Lorem.Words()));
        RuleFor(p => p.Env, f => new Dictionary<string, string>());
        RuleFor(p => p.Configure, _ => Array.Empty<Action<CreateContainerParameters>>());
    }
}