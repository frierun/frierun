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

        var result = graph.Apply(
            rootContract.Id,
            [
                rootContract with { DependsOn = [emptyContract], Handler = Handler<PackageHandler>() },
                queuedContract
            ]
        );

        Assert.True(result);
        Assert.Equal((queuedContract, queuedContract), graph.Next());
        Assert.Equal((emptyContract, null), graph.Next());
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_SameEmptyContract_ReturnsNull()
    {
        var rootContract = Factory<Package>().Generate();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            rootContract.Id,
            [
                rootContract with { DependsOn = [rootContract], Handler = Handler<PackageHandler>() }
            ]
        );

        Assert.True(result);
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_ContractReinitialization_ReturnsContract()
    {
        var rootContract = Factory<Package>().Generate();
        var childContract = Factory<Package>().Generate();
        var graph = new DiscoveryGraph();

        Assert.True(
            graph.Apply(rootContract.Id, [rootContract with { Handler = Handler<PackageHandler>() }, childContract])
        );
        var (contractId, contract) = graph.Next();
        Assert.Equal(childContract.Id, contractId);
        Assert.NotNull(contract);

        Assert.True(
            graph.Apply(childContract.Id, [childContract with { Handler = Handler<PackageHandler>() }, rootContract])
        );
        (contractId, contract) = graph.Next();
        Assert.Equal(rootContract.Id, contractId);
        Assert.NotNull(contract);
    }

    [Fact]
    public void Next_ContractInfiniteRecursion_ThrowsException()
    {
        var rootContract = Factory<Package>().Generate();
        var childContract = Factory<Package>().Generate();
        var graph = new DiscoveryGraph();

        Assert.Throws<Exception>(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    Assert.True(
                        graph.Apply(
                            rootContract.Id,
                            [rootContract with { Handler = Handler<PackageHandler>() }, childContract]
                        )
                    );
                    graph.Next();
                }
            }
        );
    }

    [Fact]
    public void Apply_UninitializedContract_MergeContract()
    {
        var contract = Factory<Parameter>().Generate();
        var graph = new DiscoveryGraph();
        var package = Factory<Package>().Generate();
        graph.Apply(
            package.Id,
            [
                package with { Handler = Handler<ParameterHandler>() },
                contract with { Value = new Argument<string>() }
            ]
        );
        var (nextContractId, nextContract) = graph.Next();
        Assert.Equal(contract.Id, nextContractId);
        Assert.NotNull(nextContract);
        Assert.Null(((Parameter)nextContract).Value.Value);

        Assert.True(
            graph.Apply(contract.Id, [contract with { DefaultValue = null, Handler = Handler<ParameterHandler>() }])
        );

        var resultContract = (Parameter)graph.Contracts[contract];
        Assert.Equal(contract.Value, resultContract.Value);
        Assert.Equal(contract.DefaultValue, resultContract.DefaultValue);
    }


    [Fact]
    public void Apply_ExistingContract_MergeContract()
    {
        var contract = Factory<Parameter>().Generate() with { Handler = Handler<ParameterHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(contract.Id, [contract with { DefaultValue = null }]));
        Assert.True(graph.Apply(contract.Id, [contract with { Value = new Argument<string>() }]));

        var resultContract = (Parameter)graph.Contracts[contract];
        Assert.Equal(contract.Value, resultContract.Value);
        Assert.Equal(contract.DefaultValue, resultContract.DefaultValue);
    }

    [Fact]
    public void Apply_ExistingConflictingContract_ReturnsFalse()
    {
        var contract = Factory<Parameter>().Generate() with { Handler = Handler<ParameterHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(contract, [contract]));
        Assert.False(graph.Apply(contract, [contract with { Value = contract.Value + "conflict" }]));
    }


    [Fact]
    public void Apply_UpdatedContract_ContractIsReinitialized()
    {
        var rootContract = Factory<Package>().Generate();
        var childContract = Factory<Parameter>().Generate();
        var prefix = Resolve<Faker>().Lorem.Word();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            rootContract, [rootContract with { Prefix = null, Handler = Handler<PackageHandler>() }]
        );
        Assert.True(result);

        result = graph.Apply(
            childContract,
            [
                childContract with { Handler = Handler<ParameterHandler>() },
                rootContract with { Prefix = prefix }
            ]
        );
        Assert.True(result);

        var (contractId, contract) = graph.Next();
        Assert.Equal(rootContract.Id, contractId);
        Assert.NotNull(contract);
        Assert.Equal(prefix, ((Package)contract).Prefix);
    }

    [Fact]
    public void Apply_ConflictingContracts_ReturnsFalse()
    {
        var rootContract = Factory<Package>().Generate();
        var childContract = Factory<Parameter>().Generate();
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(rootContract, [rootContract with { Handler = Handler<PackageHandler>() }]));
        var result = graph.Apply(
            childContract,
            [
                childContract with { Handler = Handler<ParameterHandler>() },
                rootContract with { Prefix = rootContract.Prefix + "_conflict" }
            ]
        );

        Assert.False(result);
    }
}