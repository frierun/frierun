using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Data;

public class DiscoveryGraphTests : BaseTests
{
    [Fact]
    public void Next_QueueAndEmptyContracts_QueueHasPriority()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var (emptyId, _) = Contract<Parameter>().GenerateEntry();
        var (queuedId, queuedContract) = Contract<Parameter>().GenerateEntry();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            rootId,
            new ContractList
            {
                [rootId] = rootContract with { DependsOn = [emptyId], Handler = Handler<ParameterHandler>() },
                [queuedId] = queuedContract
            }
        );

        Assert.True(result);
        Assert.Equal((queuedId, queuedContract), graph.Next());
        Assert.Equal((emptyId, null), graph.Next());
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_SameEmptyContract_ReturnsNull()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            rootId,
            new ContractList
            {
                [rootId] = rootContract with { DependsOn = [rootContract], Handler = Handler<PackageHandler>() }
            }
        );

        Assert.True(result);
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_ContractReinitialization_ReturnsContract()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var (childId, childContract) = Contract<Parameter>().GenerateEntry();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            rootId,
            new ContractList
            {
                [rootId] = rootContract with { Handler = Handler<PackageHandler>() },
                [childId] = childContract
            }
        );
        Assert.True(result);
        var (contractId, contract) = graph.Next();
        Assert.Equal(childId, contractId);
        Assert.NotNull(contract);

        result = graph.Apply(
            childId,
            new ContractList
            {
                [childId] = childContract with { Handler = Handler<PackageHandler>() },
                [rootId] = rootContract
            }
        );

        Assert.True(result);
        (contractId, contract) = graph.Next();
        Assert.Equal(rootId, contractId);
        Assert.NotNull(contract);
    }

    [Fact]
    public void Next_ContractInfiniteRecursion_ThrowsException()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var (childId, childContract) = Contract<Parameter>().GenerateEntry();
        var graph = new DiscoveryGraph();

        Assert.Throws<Exception>(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var result = graph.Apply(
                        rootId,
                        new ContractList
                        {
                            [rootId] = rootContract with { Handler = Handler<PackageHandler>() },
                            [childId] = childContract
                        }
                    );
                    Assert.True(result);
                    graph.Next();
                }
            }
        );
    }

    [Fact]
    public void Apply_UninitializedContract_MergeContract()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var (childId, childContract) = Contract<Parameter>().GenerateEntry();

        var graph = new DiscoveryGraph();
        graph.Apply(
            rootId,
            new ContractList
            {
                [rootId] = rootContract with { Handler = Handler<ParameterHandler>() },
                [childId] = childContract with { Value = new Argument<string>() }
            }
        );
        var (nextContractId, nextContract) = graph.Next();
        Assert.Equal(childId, nextContractId);
        Assert.NotNull(nextContract);
        Assert.Null(((Parameter)nextContract).Value.Value);


        var result = graph.Apply(
            childId,
            new ContractList
            {
                [childId] = childContract with { DefaultValue = null, Handler = Handler<ParameterHandler>() }
            }
        );
        Assert.True(result);

        var resultContract = (Parameter)graph.Contracts[childId];
        Assert.Equal(childContract.Value, resultContract.Value);
        Assert.Equal(childContract.DefaultValue, resultContract.DefaultValue);
    }


    [Fact]
    public void Apply_ExistingContract_MergeContract()
    {
        var (id, contract) = Contract<Parameter>().GenerateEntry();
        contract = contract with { Handler = Handler<ParameterHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(id, new ContractList { [id] = contract with { DefaultValue = null } }));
        Assert.True(graph.Apply(id, new ContractList { [id] = contract with { Value = new Argument<string>() } }));

        var resultContract = (Parameter)graph.Contracts[id];
        Assert.Equal(contract.Value, resultContract.Value);
        Assert.Equal(contract.DefaultValue, resultContract.DefaultValue);
    }

    [Fact]
    public void Apply_ExistingConflictingContract_ReturnsFalse()
    {
        var (id, contract) = Contract<Parameter>().GenerateEntry();
        contract = contract with { Handler = Handler<ParameterHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(id, new ContractList { [id] = contract }));
        Assert.False(
            graph.Apply(id, new ContractList { [id] = contract with { Value = contract.Value + "conflict" } })
        );
    }


    [Fact]
    public void Apply_UpdatedContract_ContractIsReinitialized()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var (childId, childContract) = Contract<Parameter>().GenerateEntry();
        var value = Resolve<Faker>().Lorem.Word();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            rootId,
            new ContractList
            {
                [rootId] = rootContract with { Value = new Argument<string>(), Handler = Handler<ParameterHandler>() }
            }
        );
        Assert.True(result);
        Assert.Null(graph.Next().Id);

        result = graph.Apply(
            childId,
            new ContractList
            {
                [childId] = childContract with { Handler = Handler<ParameterHandler>() },
                [rootId] = rootContract with { Value = value }
            }
        );
        Assert.True(result);

        var (contractId, contract) = graph.Next();
        Assert.Equal(rootId, contractId);
        Assert.NotNull(contract);
        Assert.Equal(value, ((Parameter)contract).Value);
    }

    [Fact]
    public void Apply_ConflictingContracts_ReturnsFalse()
    {
        var (rootId, rootContract) = Contract<Parameter>().GenerateEntry();
        var (childId, childContract) = Contract<Parameter>().GenerateEntry();
        var graph = new DiscoveryGraph();

        graph.Apply(
            rootId, new ContractList { [rootId] = rootContract with { Handler = Handler<ParameterHandler>() } }
        );

        var result = graph.Apply(
            childId,
            new ContractList
            {
                [childId] = childContract with { Handler = Handler<ParameterHandler>() },
                [rootId] = rootContract with { Value = rootContract.Value + "_conflict" }
            }
        );

        Assert.False(result);
    }
}