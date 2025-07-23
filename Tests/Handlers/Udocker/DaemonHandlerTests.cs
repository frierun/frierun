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
        var package = Factory<Package>().Generate() with { Contracts = [Factory<Daemon>().Generate()] };
        
        var application = InstallPackage(package);

        var daemon = application.Contracts.OfType<Daemon>().Single();
        Assert.True(daemon.Installed);
        var directory = "/data/data/com.termux/files/usr/var/service/" + daemon.DaemonName;
        SftpClient.Received(1).CreateDirectory(directory);
        SftpClient.Received(1).WriteAllText(directory + "/run", Arg.Any<string>());
        SshClient.Received(1).RunCommand(Arg.Is<string>(arg => arg.StartsWith("sv-enable") && arg.Contains(daemon.DaemonName)));
    }

    [Fact]
    public void Uninstall_Contract_StopsDaemon()
    {
        var package = Factory<Package>().Generate() with { Contracts = [Factory<Daemon>().Generate()] };
        var application = InstallPackage(package);
        var daemon = application.Contracts.OfType<Daemon>().Single();
        var directory = "/data/data/com.termux/files/usr/var/service/" + daemon.DaemonName;
        Assert.True(daemon.Installed);
        
        UninstallApplication(application);
        
        SshClient.Received(1).RunCommand(Arg.Is<string>(arg => arg.StartsWith("sv-disable") && arg.Contains(daemon.DaemonName)));
        SshClient.Received(1).RunCommand(Arg.Is<string>(arg => arg.StartsWith("rm -rf") && arg.Contains(directory)));
    }
}