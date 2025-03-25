using Frierun.Server.Data;
using Frierun.Server.Installers;

namespace Frierun.Tests.Data;

public class DiscoveryGraphTests : BaseTests
{
    [Fact]
    public void Next_QueueAndEmptyContracts_QueueHasPriority()
    {
        var rootContract = Factory<Package>().Generate();
        var emptyContract = Factory<Package>().Generate();
        var queuedContract = Factory<Package>().Generate();
        var graph = new DiscoveryGraph();

        graph.Apply(
            new InstallerInitializeResult(
                rootContract with { DependsOn = [emptyContract] },
                [queuedContract]
            )
        );

        Assert.Equal((queuedContract.Id, queuedContract), graph.Next());
        Assert.Equal((emptyContract.Id, null), graph.Next());
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_SameContractQueued_ReturnsNull()
    {
        var rootContract = Factory<Package>().Generate();
        var graph = new DiscoveryGraph();

        graph.Apply(
            new InstallerInitializeResult(
                rootContract,
                [rootContract]
            )
        );

        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_SameEmptyContract_ReturnsNull()
    {
        var rootContract = Factory<Package>().Generate();
        var graph = new DiscoveryGraph();

        graph.Apply(
            new InstallerInitializeResult(
                rootContract with { DependsOn = [rootContract] }
            )
        );

        Assert.Equal((null, null), graph.Next());
    }
}