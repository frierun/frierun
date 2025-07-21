using Frierun.Server.Data;
using Renci.SshNet;

namespace Frierun.Server.Handlers;

public interface ISshConnectionHandler : IHandler
{
    /// <summary>
    /// Creates an ssh client from the contract 
    /// </summary>
    public ISshClient CreateSshClient(SshConnection contract);

    /// <summary>
    /// Creates a sftp client from the contract
    /// </summary>
    public ISftpClient CreateSftpClient(SshConnection contract);
}