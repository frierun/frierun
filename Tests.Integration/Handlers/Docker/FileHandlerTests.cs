using Frierun.Server.Data;
using File = Frierun.Server.Data.File;

namespace Tests.Integration.Handlers.Docker;

public class FileHandlerTests : TestWithDocker
{
    private async Task InstallAndCheck(File contract, Func<string, Task> checkContainer)
    {
        var package = new Package(
            Name: "test-package",
            Contracts:
            [
                new Container(
                    ImageName: "alpine:latest",
                    Command: ["tail", "-f", "/dev/null"],
                    Mounts: new Dictionary<string, ContainerMount> {{"/mnt", new ContainerMount()}}
                ),
                contract
            ]
        );

        var application = InstallPackage(package);

        var volume = application.Contracts.OfType<Volume>().Single();
        Assert.True(volume.Installed);
        Assert.NotNull(volume.VolumeName);
        var container = application.Contracts.OfType<Container>().Single();
        Assert.True(container.Installed);

        await checkContainer(container.ContainerName);

        UninstallApplication(application);
    }

    [Fact]
    public async Task Install_FileWithText_PutsFile()
    {
        await InstallAndCheck(
            new File(
                Path: "test-file",
                Text: "test-text"
            ),
            async containerName =>
            {
                var (stdout, _) = await DockerService.ExecInContainer(
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
                Path: "test-file",
                Text: "test-text",
                Owner: 1000
            ),
            async containerName =>
            {
                var (stdout, _) = await DockerService.ExecInContainer(
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
                Path: "test-file",
                Text: "test-text",
                Group: 1000
            ),
            async containerName =>
            {
                var (stdout, _) = await DockerService.ExecInContainer(
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
                Path: "",
                Owner: 1000,
                Group: 1000
            ),
            async containerName =>
            {
                var (stdout, _) = await DockerService.ExecInContainer(
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

    [Fact]
    public void Install_LocalPath_PutsFile()
    {
        var directory = Directory.CreateTempSubdirectory();
        var fileName = "test-file.txt";
        var filePath = Path.Combine(directory.FullName, fileName);
        var package = new Package(
            Name: "test-package",
            Contracts:
            [
                new Container(
                    ImageName: "alpine:latest",
                    Command: ["tail", "-f", "/dev/null"],
                    Mounts: new Dictionary<string, ContainerMount> {{"/mnt", new ContainerMount()}}
                ),
                new Volume(Name: "", LocalPath: directory.FullName),
                new File(
                    Path: fileName,
                    Text: "test-text"
                ),
            ]
        );

        var application = InstallPackage(package);

        Assert.True(System.IO.File.Exists(filePath));
        Assert.Equal("test-text", System.IO.File.ReadAllText(filePath));

        UninstallApplication(application);

        directory.Delete(true);
    }
}