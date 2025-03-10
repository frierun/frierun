using Frierun.Server;
using Frierun.Server.Data;
using Microsoft.Extensions.DependencyInjection;
using File = Frierun.Server.Data.File;

namespace Tests.Integration.Installers.Docker;

public class FileInstallerTests : BaseTests
{
    private async Task InstallAndCheck(File contract, Func<string, Task> checkContainer)
    {
        var state = Services.GetRequiredService<State>();
        var package = new Package(
            Name: "test-package",
            Contracts:
            [
                new Container(
                    ImageName: "alpine:latest",
                    Command: ["tail", "-f", "/dev/null"]
                ),
                new Mount(Path: "/mnt"),
                contract
            ]
        );

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var volume = application.DependsOn.OfType<DockerVolume>().FirstOrDefault();
        Assert.NotNull(volume);
        var container = application.DependsOn.OfType<DockerContainer>().First();

        await checkContainer(container.Name);

        UninstallApplication(application);
        Assert.Empty(state.Resources);
    }

    [Fact]
    public async Task Install_FileWithText_PutsFile()
    {
        await InstallAndCheck(
            new File(
                Path: "/test-file",
                Text: "test-text"
            ),
            async containerName =>
            {
                var dockerService = Services.GetRequiredService<DockerService>();
                var (stdout, _) = await dockerService.ExecInContainer(
                    containerName,
                    [
                        "cat",
                        "/mnt/test-file"
                    ]
                );
                Assert.Equal("test-text", stdout.Trim());
            }
        );
    }

    [Fact]
    public async Task Install_FileOwner_ChownFile()
    {
        await InstallAndCheck(
            new File(
                Path: "/test-file",
                Text: "test-text",
                Owner: 1000
            ),
            async containerName =>
            {
                var dockerService = Services.GetRequiredService<DockerService>();
                var (stdout, _) = await dockerService.ExecInContainer(
                    containerName,
                    [
                        "stat",
                        "-c",
                        "%u-%g",
                        "/mnt/test-file"
                    ]
                );
                Assert.Equal("1000-0", stdout.Trim());
            }
        );
    }
    
    [Fact]
    public async Task Install_FileGroup_ChgrpFile()
    {
        await InstallAndCheck(
            new File(
                Path: "/test-file",
                Text: "test-text",
                Group: 1000
            ),
            async containerName =>
            {
                var dockerService = Services.GetRequiredService<DockerService>();
                var (stdout, _) = await dockerService.ExecInContainer(
                    containerName,
                    [
                        "stat",
                        "-c",
                        "%u-%g",
                        "/mnt/test-file"
                    ]
                );
                Assert.Equal("0-1000", stdout.Trim());
            }
        );
    }
    
    [Fact]
    public async Task Install_RootPermissions_SetPermissions()
    {
        await InstallAndCheck(
            new File(
                Path: "/",
                Owner: 1000,
                Group: 1000
            ),
            async containerName =>
            {
                var dockerService = Services.GetRequiredService<DockerService>();
                var (stdout, _) = await dockerService.ExecInContainer(
                    containerName,
                    [
                        "stat",
                        "-c",
                        "%u-%g",
                        "/mnt"
                    ]
                );
                Assert.Equal("1000-1000", stdout.Trim());
            }
        );
    }   
}