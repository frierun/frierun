using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Handlers.Udocker;

public class DaemonHandlerTests : BaseTests
{
    public DaemonHandlerTests()
    {
        InstallPackage("termux-udocker");
    }

    [Fact]
    public void Install_Contract_CreatesFilesAndDirectories()
    {
        var daemon = Contract<Daemon>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [daemon] };

        var application = InstallPackage(package);

        var installedDaemon = State.GetContract(application, daemon.Ref);
        Assert.True(installedDaemon.Installed);
        var directory = "/data/data/com.termux/files/usr/var/service/" + installedDaemon.DaemonName;
        SftpClient.Received(1).CreateDirectory(directory);
        SftpClient.Received(1).WriteAllText(directory + "/run", Arg.Any<string>());
        SshClient.Received(1).RunCommand(
            Arg.Is<string>(arg => arg.StartsWith("sv-enable") && arg.Contains(installedDaemon.DaemonName))
        );
    }

    [Fact]
    public void Uninstall_Contract_StopsDaemon()
    {
        var daemon = Contract<Daemon>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [daemon] };
        var application = InstallPackage(package);
        var installedDaemon = State.GetContract(application, daemon.Ref);
        var directory = "/data/data/com.termux/files/usr/var/service/" + installedDaemon.DaemonName;
        Assert.True(installedDaemon.Installed);

        UninstallApplication(application);

        SshClient.Received(1).RunCommand(
            Arg.Is<string>(arg => arg.StartsWith("sv-disable") && arg.Contains(installedDaemon.DaemonName))
        );
        SshClient.Received(1).RunCommand(Arg.Is<string>(arg => arg.StartsWith("rm -rf") && arg.Contains(directory)));
    }
}