using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Renci.SshNet;

namespace Frierun.Tests.Handlers;

public class FakeSshConnectionHandler : Handler<SshConnection>, ISshConnectionHandler
{
    public ISshClient SshClient { get; } = NSubstitute.Substitute.For<ISshClient>();
    public ISftpClient SftpClient { get; } = NSubstitute.Substitute.For<ISftpClient>();
    
    public ISshClient CreateSshClient(SshConnection contract)
    {
        return SshClient;
    }

    public ISftpClient CreateSftpClient(SshConnection contract)
    {
        return SftpClient;
    }
}