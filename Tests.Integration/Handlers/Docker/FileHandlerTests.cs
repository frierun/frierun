using Frierun.Server.Data;
using File = Frierun.Server.Data.File;

namespace Tests.Integration.Handlers.Docker;

public class FileHandlerTests : TestWithDocker
{
    private async Task InstallAndCheck(File contract, Func<string, Task> checkContainer, Parameter? parameter = null)
    {
        var contracts = new Dictionary<ContractRef, Contract>
        {
            {
                new ContractRef<Container>(),
                new Container(
                    ImageName: "alpine:latest",
                    Command: new Argument<IEnumerable<string>>(["tail", "-f", "/dev/null"]),
                    Mounts: new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } }
                )
            },
            {
                new ContractRef<File>(),
                contract
            }
        };

        if (parameter != null)
        {
            contracts[new ContractRef<Parameter>()] = parameter;
        }

        var package = new Package(
            Name: "test-package",
            Contracts: new ContractList(contracts)
        );

        var application = InstallPackage(package);

        var volume = Resolve<State>().GetContract<Volume>(application);
        Assert.True(volume.Installed);
        Assert.NotNull(volume.VolumeName);
        var container = Resolve<State>().GetContract<Container>(application);
        Assert.True(container.Installed);

        await checkContainer(container.ContainerName);

        UninstallApplication(application);
    }

    [Fact]
    public Task Install_FileWithText_PutsFile()
    {
        return InstallAndCheck(
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
    public Task Install_FileOwner_ChownFile()
    {
        return InstallAndCheck(
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
    public Task Install_FileGroup_ChgrpFile()
    {
        return InstallAndCheck(
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
    public Task Install_RootPermissions_SetPermissions()
    {
        return InstallAndCheck(
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
            Contracts: new ContractList
            {
                [""] = new Container(
                    ImageName: "alpine:latest",
                    Command: new Argument<IEnumerable<string>>(["tail", "-f", "/dev/null"]),
                    Mounts: new Dictionary<string, ContainerMount> { { "/mnt", new ContainerMount() } }
                ),
                [""] = new Volume(LocalPath: directory.FullName),
                [""] = new File(
                    Path: fileName,
                    Text: "test-text"
                )
            }
        );

        var application = InstallPackage(package);

        Assert.True(System.IO.File.Exists(filePath));
        Assert.Equal("test-text", System.IO.File.ReadAllText(filePath));

        UninstallApplication(application);

        directory.Delete(true);
    }

    [Fact]
    public Task Install_FileWithTemplateText_PutsFile()
    {
        return InstallAndCheck(
            new File(
                Path: "test-file",
                Text: "pre-text {{Parameter::Value}} post-text"
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
                Assert.Equal("pre-text value post-text", stdout.Trim());
            },
            new Parameter(Value: "value")
        );
    }
}