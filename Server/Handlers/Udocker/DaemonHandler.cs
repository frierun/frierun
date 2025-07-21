using System.Diagnostics;
using System.Text;
using Frierun.Server.Data;
using Renci.SshNet.Common;

namespace Frierun.Server.Handlers.Udocker;

public class DaemonHandler(Application application)
    : Handler<Daemon>(application)
{
    private const string PrefixPath = "/data/data/com.termux/files/usr";
    private const string DaemonsPath = PrefixPath + "/var/service";
    private readonly SshConnection _connection = application.Contracts.OfType<SshConnection>().Single();

    public override IEnumerable<ContractInitializeResult> Initialize(Daemon contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                DaemonName = contract.DaemonName ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    c => c.DaemonName
                ),
                Handler = this
            }
        );
    }

    public override Daemon Install(Daemon contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.DaemonName != null);

        using var sftpClient = _connection.CreateSftpClient();

        sftpClient.CreateDirectory(DaemonsPath + "/" + contract.DaemonName);

        var runContent = new StringBuilder();
        runContent.Append("#!/data/data/com.termux/files/usr/bin/sh\n\n");
        foreach (var commandPre in contract.PreCommands)
        {
            runContent.Append($"{string.Join(' ', commandPre.Select(EscapeArgument))} 2>&1\n");
        }
        runContent.Append($"exec {string.Join(' ', contract.Command.Select(EscapeArgument))} 2>&1\n");
        
        sftpClient.WriteAllText(DaemonsPath + "/" + contract.DaemonName + "/run", runContent.ToString());
        sftpClient.ChangePermissions(DaemonsPath + "/" + contract.DaemonName + "/run", 0755);

        sftpClient.CreateDirectory(DaemonsPath + "/" + contract.DaemonName + "/log");
        sftpClient.SymbolicLink(
            PrefixPath + "/share/termux-services/svlogger", DaemonsPath + "/" + contract.DaemonName + "/log/run"
        );

        using var sshClient = _connection.CreateSshClient();
        sshClient.RunCommand("sv-enable " + EscapeArgument(contract.DaemonName)).Dispose();

        return contract;
    }

    public override void Uninstall(Daemon contract)
    {
        Debug.Assert(contract.Installed);

        using var sshClient = _connection.CreateSshClient();
        sshClient.RunCommand("sv-disable " + EscapeArgument(contract.DaemonName)).Dispose();
        sshClient.RunCommand("rm -rf " + EscapeArgument(DaemonsPath + "/" + contract.DaemonName)).Dispose();
    }

    /// <summary>
    /// Escapes argument for shell using single quoting
    /// </summary>
    private string EscapeArgument(string arg)
    {
        return "'" + arg.Replace("'", "'\''") + "'";
    }
}