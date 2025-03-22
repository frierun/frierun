using Frierun.Server.Data;
using Frierun.Server.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using File = System.IO.File;

namespace Frierun.Tests.Services;

public class StateSerializerTests : BaseTests
{
    [Fact]
    public void Load_NonExistingFile_ReturnsEmptyState()
    {
        var stateManager = Resolve<StateSerializer>();
        File.Delete(stateManager.Path);

        var state = stateManager.Load();

        Assert.Empty(state.Applications);
    }

    [Fact]
    public void Load_Empty_ReturnsEmptyState()
    {
        var stateManager = Resolve<StateSerializer>();
        File.WriteAllText(stateManager.Path, "");

        var state = stateManager.Load();

        Assert.Empty(state.Applications);
    }
    
    [Fact]
    public void Load_FileWithApplication_ReturnsNewInstanceOfApplication()
    {
        var application = Factory<Application>().Generate();
        var state = new State();
        state.AddApplication(application);
        var stateManager = Resolve<StateSerializer>();
        stateManager.Save(state);
        
        var loadedState = stateManager.Load();

        Assert.Single(loadedState.Applications);
        Assert.Equal(application.Name, loadedState.Applications.First().Name);
        Assert.NotSame(application, loadedState.Applications.First());
    }    

    [Fact]
    public void Load_FileWithApplication_ReturnsSameInstanceOfPackage()
    {
        var application = Factory<Application>().Generate();
        var state = new State();
        state.AddApplication(application);
        var stateManager = Resolve<StateSerializer>();
        stateManager.Save(state);
        
        var loadedState = stateManager.Load();

        // Package must be deserialized by reference
        Assert.Same(application.Package, loadedState.Applications.First().Package); 
    }

    [Fact]
    public void Save_StateWithApplication_DoesntSerializePackageContent()
    {
        var application = Factory<Application>().Generate();
        var state = new State();
        state.AddApplication(application);
        var stateManager = Resolve<StateSerializer>();

        stateManager.Save(state);

        var content = File.ReadAllText(stateManager.Path);
        Assert.NotNull(application.Package);
        Assert.NotNull(application.Package.Url);
        Assert.DoesNotContain(application.Package.Url, content);
    }
}