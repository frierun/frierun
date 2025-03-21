using Frierun.Server.Data;

namespace Frierun.Tests.Installers.Base;

public class DependencyInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_TwoContainerWithDependency_CorrectOrder(bool reverseOrder)
    {
        var containers = Factory<Container>().Generate(2);
        List<Contract> contracts =
        [
            containers[0],
            containers[1],
            new Dependency(containers[0].Id, containers[1].Id)
        ];
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
        var installedContainers = application.DependsOn.OfType<DockerContainer>();
        Assert.Equal(
            [containers[0].ContainerName, containers[1].ContainerName],
            installedContainers.Select(c => c.Name)
        );
    }
}