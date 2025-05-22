using Frierun.Server.Data;

namespace Tests.Integration;

public class PodmanTests : BaseTests
{
    [Fact]
    public void Install_PodmanSocket_DetectsCorrectRootPath()
    {
        // default windows socket
        //const string socketUri = "npipe://./pipe/podman-machine-default";
        const string socketUri = "unix:/run/podman/podman.sock";
        const string socketPath = "/run/podman/podman.sock";
        var application = InstallPackage(
            "docker",
            [new Parameter(Name: "Uri", Value: socketUri)]
        );

        var dockerApiConnection = application.Contracts.OfType<DockerApiConnection>().Single();
        Assert.True(dockerApiConnection.Installed);
        Assert.True(dockerApiConnection.IsPodman);
        Assert.Equal(socketPath, dockerApiConnection.GetSocketRootPath());

        UninstallApplication(application);
    }
}