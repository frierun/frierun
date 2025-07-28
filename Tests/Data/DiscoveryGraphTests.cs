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
            new ContractInitializeResult(
                rootContract with { DependsOn = [emptyContract], Handler = Handler<PackageHandler>() },
                [queuedContract]
            )
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
            new ContractInitializeResult(
                rootContract with { DependsOn = [rootContract], Handler = Handler<PackageHandler>() }
            )
        );

        Assert.True(result);
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Next_ContractReinitialization_ReturnsContract()
    {
        var rootContract = Factory<Package>().Generate() with { Handler = Handler<PackageHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(new ContractInitializeResult(rootContract, [rootContract])));

        var (contractId, contract) = graph.Next();
        Assert.Equal(rootContract.Id, contractId);
        Assert.NotNull(contract);
    }

    [Fact]
    public void Next_ContractReinitializationRecursion_ThrowsException()
    {
        var rootContract = Factory<Package>().Generate() with { Handler = Handler<PackageHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(new ContractInitializeResult(rootContract, [rootContract])));
        graph.Next();
        Assert.True(graph.Apply(new ContractInitializeResult(rootContract, [rootContract])));

        Assert.Throws<Exception>(() => graph.Next());
    }

    [Fact]
    public void Apply_UpdatedContract_ContractIsReinitialized()
    {
        var rootContract = Factory<Package>().Generate();
        var childContract = Factory<Parameter>().Generate();
        var prefix = Resolve<Faker>().Lorem.Word();
        var graph = new DiscoveryGraph();

        var result = graph.Apply(
            new ContractInitializeResult(
                rootContract with { Prefix = null, Handler = Handler<PackageHandler>() }
            )
        );
        Assert.True(result);

        result = graph.Apply(
            new ContractInitializeResult(
                childContract with { Handler = Handler<ParameterHandler>() },
                [rootContract with { Prefix = prefix }]
            )
        );
        Assert.True(result);

        var (contractId, contract) = graph.Next();
        Assert.Equal(rootContract.Id, contractId);
        Assert.NotNull(contract);
        Assert.Equal(prefix, ((Package)contract).Prefix);
    }

    [Fact]
    public void Apply_ContractWithIHasStrings_AddedSubstitute()
    {
        var rootContract = Factory<Package>().Generate() with
        {
            Handler = Handler<PackageHandler>(),
            ApplicationDescription = "{{Parameter:Test:Value}}"
        };
        var rootSubstitute = new Substitute(rootContract);
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(new ContractInitializeResult(rootContract)));

        var (substituteId, substitute) = graph.Next();
        Assert.Equal(rootSubstitute.Id, substituteId);
        Assert.NotNull(substitute);
        Assert.Single(((Substitute)substitute).Matches);
        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Apply_ContractWithIHasStringsWithoutSubstitutes_NoSubstituteAdded()
    {
        var rootContract = Factory<Package>().Generate() with
        {
            Handler = Handler<PackageHandler>()
        };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(new ContractInitializeResult(rootContract)));

        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Apply_ContractWithIHasStringsIsUpdated_SubstituteRefreshed()
    {
        var rootContract = Factory<Package>().Generate() with
        {
            Handler = Handler<PackageHandler>(),
            ApplicationDescription = "{{Parameter:Description:Value}}",
            ApplicationUrl = null,
        };
        var rootSubstitute = new Substitute(rootContract);
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(new ContractInitializeResult(rootContract)));
        var result = graph.Apply(
            new ContractInitializeResult(
                rootContract with { ApplicationUrl = "{{Parameter:Url:Value}}" }
            )
        );
        Assert.True(result);

        var (contractId, contract) = graph.Next();
        Assert.Equal(rootSubstitute.Id, contractId);
        Assert.NotNull(contract);
        Assert.Equal(2, ((Substitute)contract).Matches.Count);

        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Apply_ConflictingContracts_ReturnsFalse()
    {
        var rootContract = Factory<Package>().Generate() with { Handler = Handler<PackageHandler>() };
        var childContract = Factory<Parameter>().Generate() with { Handler = Handler<ParameterHandler>() };
        var graph = new DiscoveryGraph();

        Assert.True(graph.Apply(new ContractInitializeResult(rootContract)));
        var result = graph.Apply(
            new ContractInitializeResult(
                childContract, 
                [rootContract with { Prefix = rootContract.Prefix + "_conflict" }]
            )
        );

        Assert.False(result);
    }
}