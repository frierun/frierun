using Frierun.Server.Models;
using Frierun.Server.Services;
using Frierun.Server.Services.Serialization;

namespace Frierun.Tests.Services.Serialization;

public class StateManagerTests : BaseTests
{
    [Fact]
    public void DeserializesEmptyState()
    {
        var stateManager = GetService<StateManager>();

        if (File.Exists(stateManager.Path))
        {
            File.Delete(stateManager.Path);
        }

        var state = stateManager.Load();

        Assert.Empty(state.Applications);
    }

    [Fact]
    public void DeserializesApplicationAndPackage()
    {
        var package = new Package("test", "test2", 80);
        var packageRegistry = GetService<PackageRegistry>();
        packageRegistry.Packages.Add(package);
        
        var application = new Application(Guid.NewGuid(), package);
        var state = new State();
        state.Applications.Add(application);
        var stateManager = GetService<StateManager>();
        
        stateManager.Save(state);
        var loadedState = stateManager.Load();

        Assert.Single(loadedState.Applications);
        Assert.Equal(application.Id, loadedState.Applications[0].Id);
        Assert.Equal(package, loadedState.Applications[0].Package); // Package should be deserialized by reference
    }

    [Fact]
    public void DoesntSerializePackageContent()
    {
        var package = new Package("test", "testimagename", 80);
        var application = new Application(Guid.NewGuid(), package);
        var state = new State();
        state.Applications.Add(application);
        var stateManager = GetService<StateManager>();

        stateManager.Save(state);

        var content = File.ReadAllText(stateManager.Path);
        Assert.DoesNotContain("testimagename", content);
    }
}