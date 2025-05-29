using Frierun.Server;
using Frierun.Server.Data;
using File = System.IO.File;

namespace Frierun.Tests;

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

    [Fact]
    public void Load_FrierunWithTraefikEndpoint_Serialized()
    {
        var stateManager = Resolve<StateSerializer>();
        var state = Resolve<State>();
        InstallPackage("static-domain");
        InstallPackage("docker");
        InstallPackage("traefik");
        InstallPackage("frierun");

        var loadedState = stateManager.Load();

        Assert.NotEmpty(loadedState.Applications);
        Assert.Equal(
            state.Contracts.OfType<HttpEndpoint>().Single().NetworkName,
            loadedState.Contracts.OfType<HttpEndpoint>().Single().NetworkName
        );
    }

    [Fact]
    public void Load_FrierunWithDockerVolume_Serialized()
    {
        var stateManager = Resolve<StateSerializer>();
        var state = Resolve<State>();
        InstallPackage("docker");
        InstallPackage(
            "frierun",
            [
                new Volume("config", VolumeName: "test"),
            ]
        );

        var loadedState = stateManager.Load();

        Assert.NotEmpty(loadedState.Applications);
        Assert.NotNull(state.Contracts.OfType<Volume>().Single().VolumeName);
        Assert.NotNull(loadedState.Contracts.OfType<Volume>().Single().VolumeName);
    }

    [Fact]
    public void Load_FrierunWithLocalPath_Serialized()
    {
        var stateManager = Resolve<StateSerializer>();
        var state = Resolve<State>();
        InstallPackage("docker");
        InstallPackage(
            "frierun",
            [
                new Volume("config", LocalPath: "/test"),
            ]
        );

        var loadedState = stateManager.Load();

        Assert.NotEmpty(loadedState.Applications);
        Assert.NotNull(state.Contracts.OfType<Volume>().Single().LocalPath);
        Assert.NotNull(loadedState.Contracts.OfType<Volume>().Single().LocalPath);
    }
}