using System.Diagnostics;
using Frierun.Server.Handlers;
using Renci.SshNet;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record SshConnection(
    string? Host = null,
    int Port = 0,
    string? Username = null,
    string? Password = null
)
    : Contract<ISshConnectionHandler>
{
    /// <summary>
    /// Create an ssh client from the contract.
    /// </summary>
    public ISshClient CreateSshClient()
    {
        Debug.Assert(Handler != null);
        return Handler.CreateSshClient(this);
    }

    /// <summary>
    /// Creates a sftp client from the contract.
    /// </summary>
    public ISftpClient CreateSftpClient()
    {
        Debug.Assert(Handler != null);
        return Handler.CreateSftpClient(this);
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Host = OnlyOne(Host, contract.Host),
            Port = OnlyOne(Port, contract.Port, port => port == 0),
            Username = OnlyOne(Username, contract.Username),
            Password = OnlyOne(Password, contract.Password)
        };
    }
    
    /// <summary>
    /// Escapes argument for shell using single quoting
    /// </summary>
    public static string EscapeArgument(string arg)
    {
        return "'" + arg.Replace("'", "'\''") + "'";
    }
}