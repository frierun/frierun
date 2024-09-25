using Frierun.Server.Models;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class StateManagerTests
{
    [Fact]
    public void DeserializesApplication()
    {
        var application = new Application(Guid.NewGuid(), new Package("test", "test", 80));
        var state = new State();
        state.Applications.Add(application);

        var stateManager = new StateManager();
        stateManager.Save(state);
        var loadedState = stateManager.Load();
        
        Assert.Single(loadedState.Applications);
        Assert.Equal(application.Id, loadedState.Applications[0].Id);
    }
}