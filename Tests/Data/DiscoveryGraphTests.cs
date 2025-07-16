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

        Assert.Equal((queuedContract, queuedContract), graph.Next());
        Assert.Equal((emptyContract, null), graph.Next());
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

        graph.Apply(new ContractInitializeResult(rootContract));

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

        graph.Apply(new ContractInitializeResult(rootContract));

        Assert.Equal((null, null), graph.Next());
    }

    [Fact]
    public void Apply_ContractWithIHasStringsIsUpdated_SubstituteRefreshed()
    {
        var rootContract = Factory<Package>().Generate() with
        {
            Handler = Handler<PackageHandler>(),
            ApplicationUrl = null,
            ApplicationDescription = null,
        };
        var rootSubstitute = new Substitute(rootContract);
        var graph = new DiscoveryGraph();

        graph.Apply(
            new ContractInitializeResult(
                rootContract with
                {
                    ApplicationDescription = "{{Parameter:Description:Value}}"
                }
            )
        );
        graph.Apply(
            new ContractInitializeResult(
                new Selector("") with { Handler = Handler<SelectorHandler>() },
                [rootContract with { ApplicationUrl = "{{Parameter:Url:Value}}" }]
            )
        );

        var (substituteId, substitute) = graph.Next();
        Assert.Equal(rootSubstitute.Id, substituteId);
        Assert.NotNull(substitute);
        Assert.Equal(2, ((Substitute)substitute).Matches.Count);
        Assert.Equal((null, null), graph.Next());
    }
}