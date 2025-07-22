using System.Text;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using NSubstitute;
using Renci.SshNet;

namespace Frierun.Tests.Handlers;

public class FakeSshConnectionHandler : Handler<SshConnection>, ISshConnectionHandler
{
    public ISshClient SshClient { get; } = CreateSshClientSubstitute();
    public ISftpClient SftpClient { get; } = NSubstitute.Substitute.For<ISftpClient>();

    public ISshClient CreateSshClient(SshConnection contract)
    {
        return SshClient;
    }

    public ISftpClient CreateSftpClient(SshConnection contract)
    {
        return SftpClient;
    }

    private static ISshClient CreateSshClientSubstitute()
    {
        var result = NSubstitute.Substitute.For<ISshClient>();

        var iSession = typeof(SshCommand).Assembly.GetType("Renci.SshNet.ISession");
        if (iSession == null)
        {
            throw new Exception("Could not find the ISession interface");
        }

        result.RunCommand(Arg.Any<string>()).ReturnsForAnyArgs(info =>
            NSubstitute.Substitute.For<SshCommand>(
                NSubstitute.Substitute.For([iSession], []),
                info.Arg<string>(),
                Encoding.UTF8
            )
        );

        return result;
    }
}