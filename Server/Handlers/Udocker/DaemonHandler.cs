using System.Diagnostics;
using System.Text;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class DaemonHandler(State state, Application application)
    : Handler<Daemon>(state, application)
{
    private const string PrefixPath = "/data/data/com.termux/files/usr";
    private const string DaemonsPath = PrefixPath + "/var/service";
    private readonly SshConnection _connection = application.GetContract(new ContractId<SshConnection>());

    public override IEnumerable<ContractList> Initialize(Daemon contract, ApplicationContext context)
    {
        yield return new ContractList
        {
            [context] = contract with
            {
                DaemonName = contract.DaemonName ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.DaemonName
                ),
                Handler = this
            }
        };
    }

    public override Daemon Install(Daemon contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.DaemonName != null);

        using var sftpClient = _connection.CreateSftpClient();

        sftpClient.CreateDirectory(DaemonsPath + "/" + contract.DaemonName);

        var runContent = new StringBuilder();
        runContent.Append("#!/data/data/com.termux/files/usr/bin/sh\n\n");
        if (contract.PreCommands.Value != null)
        {
            foreach (var commandPre in contract.PreCommands.Value)
            {
                runContent.Append($"{string.Join(' ', commandPre.Select(SshConnection.EscapeArgument))} 2>&1\n");
            }
        }

        if (contract.Command.Value != null)
        {
            runContent.Append(
                $"exec {string.Join(' ', contract.Command.Value.Select(SshConnection.EscapeArgument))} 2>&1\n"
            );
        }

        sftpClient.WriteAllText(DaemonsPath + "/" + contract.DaemonName + "/run", runContent.ToString());
        sftpClient.ChangePermissions(DaemonsPath + "/" + contract.DaemonName + "/run", 0755);

        sftpClient.CreateDirectory(DaemonsPath + "/" + contract.DaemonName + "/log");
        sftpClient.SymbolicLink(
            PrefixPath + "/share/termux-services/svlogger", DaemonsPath + "/" + contract.DaemonName + "/log/run"
        );

        using var sshClient = _connection.CreateSshClient();
        sshClient.RunCommand("sv-enable " + SshConnection.EscapeArgument(contract.DaemonName)).Dispose();

        return contract;
    }

    public override void Uninstall(Daemon contract)
    {
        Debug.Assert(contract.Installed);

        using var sshClient = _connection.CreateSshClient();
        sshClient.RunCommand("sv-disable " + SshConnection.EscapeArgument(contract.DaemonName)).Dispose();
        sshClient.RunCommand("rm -rf " + SshConnection.EscapeArgument(DaemonsPath + "/" + contract.DaemonName))
            .Dispose();
    }
}