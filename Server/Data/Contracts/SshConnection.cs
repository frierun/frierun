using System.Diagnostics;
using Frierun.Server.Handlers;
using Renci.SshNet;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record SshConnection(
    string? Name = null,
    string? Host = null,
    int Port = 0,
    string? Username = null,
    string? Password = null
)
    : Contract<ISshConnectionHandler>(Name ?? ""), IHasStrings
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
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Host = OnlyOne(Host, contract.Host),
            Port = OnlyOne(Port, contract.Port, port => port == 0),
            Username = OnlyOne(Username, contract.Username),
            Password = OnlyOne(Password, contract.Password)
        };
    }

    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Host = Host == null ? null : decorator(Host),
            Username = Username == null ? null : decorator(Username),
            Password = Password == null ? null : decorator(Password)
        };
    }
}