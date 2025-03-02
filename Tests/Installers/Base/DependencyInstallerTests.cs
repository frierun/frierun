using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class DependencyInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_TwoContainerWithDependency_CreatesDependency(bool reverseOrder)
    {
        var containers = Factory<Container>().Generate(2);
        var contracts = new List<Contract>(
            [
                containers[0],
                containers[1],
                new Dependency(containers[0].Id, containers[1].Id)
            ]
        );
        if (reverseOrder)
        {
            contracts.Reverse();
        }

        var package = Factory<Package>().Generate() with
        {
            Contracts = contracts
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var dockerContainer = application.DependsOn
            .OfType<DockerContainer>()
            .First(container => container.Name == containers[1].ContainerName);
        Assert.Contains(
            dockerContainer.DependsOn.OfType<DockerContainer>(),
            r => r.Name == containers[0].ContainerName
        );
    }
}