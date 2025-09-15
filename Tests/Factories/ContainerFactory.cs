using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class ContainerFactory : ContractFaker<Container>
{
    public ContainerFactory()
    {
        CustomInstantiator(_ => new Container());
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        RuleFor(p => p.ImageName, f => new Argument<string>(f.Internet.Url()));
        RuleFor(p => p.MountDockerSocket, f => f.Random.Bool());
        RuleFor(p => p.NetworkName, _ => "");
        RuleFor(p => p.Command, f => new List<string>(f.Lorem.Words()));
        RuleFor(p => p.Env, f => new Dictionary<string, Argument<string>>());
        RuleFor(p => p.Labels, f => new Dictionary<string, Argument<string>>());

        RuleSet(
            "udocker", set => { set.RuleFor(p => p.MountDockerSocket, false); }
        );
    }
}