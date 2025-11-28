using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class DockerApiConnectionFactory : ContractFaker<DockerApiConnection>
{
    public DockerApiConnectionFactory()
    {
        CustomInstantiator(_ => new DockerApiConnection());
        RuleFor(p => p.Path, f => f.System.FilePath());
        RuleFor(p => p.IsPodman, f => f.Random.Bool());
    }
}