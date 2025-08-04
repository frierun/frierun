using Frierun.Server;
using Frierun.Server.Data;
using Console = Frierun.Server.Console;
using File = System.IO.File;

namespace Tests.Integration;

public class DiscoverTests : BaseTests
{
    [Fact]
    public void Execute_StateFileDeleted_CreatesStateFile()
    {
        var stateSerializer = Resolve<StateSerializer>();
        File.Delete(stateSerializer.Path);
        Assert.False(File.Exists(stateSerializer.Path));
        var console = Resolve<Console>();

        var exitCode = console.Run(["discover"]);
        
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(stateSerializer.Path));
    }
    
    [Fact]
    public void Execute_Twice_ContractsAreNotDuplicated()
    {
        var state = Resolve<State>();
        var console = Resolve<Console>();

        console.Run(["discover"]);
        var unmanageCount = state.UnmanagedContracts.Count;
        console.Run(["discover"]);
        
        Assert.Equal(unmanageCount, state.UnmanagedContracts.Count);
    }

    [Fact]
    public void Execute_StateFileDeleted_FoundsDockerSocket()
    {
        var stateSerializer = Resolve<StateSerializer>();
        File.Delete(stateSerializer.Path);
        var console = Resolve<Console>();

        console.Run(["discover"]);

        var state = Resolve<State>();
        Assert.NotEmpty(state.Contracts.OfType<DockerApiConnection>());
    }
}