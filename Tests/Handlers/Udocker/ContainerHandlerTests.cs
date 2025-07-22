using Frierun.Server.Data;
using Frierun.Server.Handlers.Udocker;

namespace Frierun.Tests.Handlers.Udocker;

public class ContainerHandlerTests : BaseTests
{
    [Fact]
    public void Initialize_ContractWithMountDockerSocket_ReturnsEmpty()
    {
        var udocker = InstallPackage("termux-udocker");
        var container = Factory<Container>().Generate() with { MountDockerSocket = true };
        var handler = Handler<ContainerHandler>(udocker);

        var result = handler.Initialize(container, "");

        Assert.Empty(result);
    }
}