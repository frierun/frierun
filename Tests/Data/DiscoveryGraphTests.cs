using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

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
            new ContractInitializeResult(
                rootContract with { DependsOn = [emptyContract], Handler = Handler<PackageHandler>() },
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
            new ContractInitializeResult(
                rootContract with { Handler = Handler<PackageHandler>() },
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
            new ContractInitializeResult(
                rootContract with { DependsOn = [rootContract], Handler = Handler<PackageHandler>() }
            )
        );

        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Apply_UpdatedContract_ContractIsMerged()
    {
        var rootContract = Factory<Package>().Generate();
        var childContract = Factory<Parameter>().Generate();
        var prefix = Resolve<Faker>().Lorem.Word();
        var graph = new DiscoveryGraph();

        graph.Apply(
            new ContractInitializeResult(
                rootContract with { Prefix = null, Handler = Handler<PackageHandler>() }
            )
        );

        graph.Apply(
            new ContractInitializeResult(
                childContract with { Handler = Handler<ParameterHandler>() },
                [rootContract with { Prefix = prefix }]
            )
        );

        var initializedRoot = (Package)graph.Contracts[rootContract];
        Assert.Equal(prefix, initializedRoot.Prefix);
    }
}