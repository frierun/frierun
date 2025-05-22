using Frierun.Server.Data;

namespace Tests.Integration;

public class PodmanTests : BaseTests
{
    [Fact]
    public void Install_PodmanSocket_DetectsCorrectRootPath()
    {
        const string path = "/run/podman/podman.sock";
        var application = InstallPackage(
            "docker",
            [new Parameter(Name: "Path", Value: path)]
        );

        var dockerApiConnection = application.Contracts.OfType<DockerApiConnection>().Single();
        Assert.True(dockerApiConnection.Installed);
        Assert.True(dockerApiConnection.IsPodman);
        Assert.Equal(path, dockerApiConnection.GetSocketRootPath());

        UninstallApplication(application);
    }
}