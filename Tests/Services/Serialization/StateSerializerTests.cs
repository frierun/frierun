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

        Assert.Empty(state.Resources);
    }

    [Fact]
    public void Load_Empty_ReturnsEmptyState()
    {
        var stateManager = Resolve<StateSerializer>();
        File.WriteAllText(stateManager.Path, "");

        var state = stateManager.Load();

        Assert.Empty(state.Resources);
    }
    
    [Fact]
    public void Load_FileWithApplication_ReturnsNewInstanceOfApplication()
    {
        var application = GetFactory<Application>().Generate();
        var state = new State();
        state.Resources.Add(application);
        var stateManager = Resolve<StateSerializer>();
        stateManager.Save(state);
        
        var loadedState = stateManager.Load();

        Assert.Single(loadedState.Resources);
        Assert.Equal(application.Id, loadedState.Resources[0].Id);
        Assert.NotSame(application, loadedState.Resources[0]);
    }    

    [Fact]
    public void Load_FileWithApplication_ReturnsSameInstanceOfPackage()
    {
        var application = GetFactory<Application>().Generate();
        var state = new State();
        state.Resources.Add(application);
        var stateManager = Resolve<StateSerializer>();
        stateManager.Save(state);
        
        var loadedState = stateManager.Load();

        // Package must be deserialized by reference
        Assert.Same(application.Package, ((Application)loadedState.Resources[0]).Package); 
    }

    [Fact]
    public void Save_StateWithApplication_DoesntSerializePackageContent()
    {
        var application = GetFactory<Application>().Generate();
        var state = new State();
        state.Resources.Add(application);
        var stateManager = Resolve<StateSerializer>();

        stateManager.Save(state);

        var content = File.ReadAllText(stateManager.Path);
        Assert.NotNull(application.Package);
        Assert.NotNull(application.Package.Url);
        Assert.DoesNotContain(application.Package.Url, content);
    }
}