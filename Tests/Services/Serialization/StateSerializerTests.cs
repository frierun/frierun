using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class StateSerializerTests : BaseTests
{
    [Fact]
    public void Load_EmptyFile_ReturnsEmptyState()
    {
        var stateManager = Resolve<StateSerializer>();
        File.Delete(stateManager.Path);

        var state = stateManager.Load();

        Assert.Empty(state.Applications);
    }
    
    [Fact]
    public void Load_FileWithApplication_ReturnsNewInstanceOfApplication()
    {
        var application = GetFactory<Application>().Generate();
        var state = new State();
        state.Applications.Add(application);
        var stateManager = Resolve<StateSerializer>();
        stateManager.Save(state);
        
        var loadedState = stateManager.Load();

        Assert.Single(loadedState.Applications);
        Assert.Equal(application.Id, loadedState.Applications[0].Id);
        Assert.NotSame(application, loadedState.Applications[0]);
    }    

    [Fact]
    public void Load_FileWithApplication_ReturnsSameInstanceOfPackage()
    {
        var application = GetFactory<Application>().Generate();
        var state = new State();
        state.Applications.Add(application);
        var stateManager = Resolve<StateSerializer>();
        stateManager.Save(state);
        
        var loadedState = stateManager.Load();

        Assert.Same(application.Package, loadedState.Applications[0].Package); // Package should be deserialized by reference
    }

    [Fact]
    public void Save_StateWithApplication_DoesntSerializePackageContent()
    {
        var application = GetFactory<Application>().Generate();
        var state = new State();
        state.Applications.Add(application);
        var stateManager = Resolve<StateSerializer>();

        stateManager.Save(state);

        var content = File.ReadAllText(stateManager.Path);
        Assert.NotNull(application.Package);
        Assert.DoesNotContain(application.Package.Url, content);
    }
}