using Bogus;
using Frierun.Server.Data;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Data;

public class DiscoveryGraphTests : BaseTests
{
    [Fact]
    public void Next_QueueAndEmptyContracts_QueueHasPriority()
    {
        var empty = Contract<Parameter>().Generate();
        var root = Contract<Parameter>()
            .SetHandler<ParameterHandler>()
            .Set(p => p.DependsOn, [empty.Ref])
            .Generate();
        var queued = Contract<Parameter>().Generate();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(root.Ref, [root, queued]);

        Assert.True(result);
        Assert.Equal((Ref: queued.Ref, queued.Contract), graph.Next());
        Assert.Equal((Ref: empty.Ref, null), graph.Next());
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_SameEmptyContract_ReturnsNull()
    {
        var root = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(root.Ref, [root]);

        Assert.True(result);
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_ContractReinitialization_ReturnsContract()
    {
        var root = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var child = Contract<Parameter>().Generate();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(root.Ref, [root, child]);
        Assert.True(result);
        var (contractRef, contract) = graph.Next();
        Assert.Equal(child.Ref, contractRef);
        Assert.NotNull(contract);

        result = graph.Apply(
            child.Ref,
            [
                child.With(c => c with { Handler = Handler<ParameterHandler>() }),
                root
            ]
        );

        Assert.True(result);
        (contractRef, contract) = graph.Next();
        Assert.Equal(root.Ref, contractRef);
        Assert.NotNull(contract);
    }

    [Fact]
    public void Next_ContractInfiniteRecursion_ThrowsException()
    {
        var root = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var child = Contract<Parameter>().Generate();
        var graph = new DiscoveryGraph();

        Assert.Throws<Exception>(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var result = graph.Apply(root.Ref, [root, child]);
                    Assert.True(result);
                    graph.Next();
                }
            }
        );
    }

    [Fact]
    public void Apply_UninitializedContract_MergeContract()
    {
        var root = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var child = Contract<Parameter>().Generate();

        var graph = new DiscoveryGraph();
        graph.Apply(
            root.Ref,
            [
                root,
                child.With(c => c with { Value = new Argument<string>() })
            ]
        );
        var (nextContractRef, nextContract) = graph.Next();
        Assert.Equal(child.Ref, nextContractRef);
        Assert.NotNull(nextContract);
        Assert.Null(((Parameter)nextContract).Value.Value);


        var result = graph.Apply(
            child.Ref,
            [
                child.With(c => c with { DefaultValue = null, Handler = Handler<ParameterHandler>() })
            ]
        );
        Assert.True(result);

        var resultContract = (Parameter)graph.Contracts[child.Ref];
        Assert.Equal(child.Contract.Value, resultContract.Value);
        Assert.Equal(child.Contract.DefaultValue, resultContract.DefaultValue);
    }


    [Fact]
    public void Apply_SameContract_MergeContract()
    {
        var contract = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(contract.Ref, [contract.With(c => c with { DefaultValue = null })]));
        Assert.True(graph.Apply(contract.Ref, [contract.With(c => c with { Value = new Argument<string>() })]));

        var resultContract = (Parameter)graph.Contracts[contract.Ref];
        Assert.Equal(contract.Contract.Value, resultContract.Value);
        Assert.Equal(contract.Contract.DefaultValue, resultContract.DefaultValue);
    }

    [Fact]
    public void Apply_SameConflictingContract_ReturnsFalse()
    {
        var contract = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(contract.Ref, [contract]));
        Assert.False(
            graph.Apply(
                contract.Ref,
                [contract.With(c => c with { Value = contract.Contract.Value + "conflict" })]
            )
        );
    }


    [Fact]
    public void Apply_UpdatedContract_ContractIsReinitialized()
    {
        var root = Contract<Parameter>()
            .Set(p => p.Value, new Argument<string>())
            .SetHandler<ParameterHandler>()
            .Generate();
        var child = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var value = Resolve<Faker>().Lorem.Word();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(root.Ref, [root]);
        Assert.True(result);
        Assert.Null(graph.Next().Ref);

        result = graph.Apply(
            child.Ref,
            [
                child,
                root.With(c => c with { Value = value })
            ]
        );
        Assert.True(result);

        var (contractRef, contract) = graph.Next();
        Assert.Equal(root.Ref, contractRef);
        Assert.NotNull(contract);
        Assert.Equal(value, ((Parameter)contract).Value);
    }

    [Fact]
    public void Apply_ConflictingContracts_ReturnsFalse()
    {
        var root = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var child = Contract<Parameter>().SetHandler<ParameterHandler>().Generate();
        var graph = new DiscoveryGraph();

        graph.Apply(root.Ref, [root]);

        var result = graph.Apply(
            child.Ref,
            [
                child,
                root.With(c => c with { Value = root.Contract.Value + "_conflict" })
            ]
        );

        Assert.False(result);
    }
}