using System.Diagnostics;
using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class MysqlHandler(State state, Application application)
    : Handler<Mysql>(state, application)
{
    private readonly Container _container = state.GetContract<Container>(application);
    private readonly string _rootPassword = state.GetContract<Password>(application).Value ??
                                            throw new Exception("Root password not found");

    public override IEnumerable<ContractList> Initialize(Mysql contract, ApplicationContext context)
    {
        if (contract.Admin)
        {
            if (contract.Username != null && contract.Username != "root")
            {
                yield break;
            }

            if (contract.Password != null && contract.Password != _rootPassword)
            {
                yield break;
            }

            yield return new ContractList
            {
                [context] = contract with
                {
                    Handler = this,
                    Username = "root",
                    Password = _rootPassword,
                    Host = _container.ContainerName,
                }
            };
        }

        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                Database = contract.Database ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.Database,
                    "",
                    ["mysql"]
                ),
                Username = contract.Username ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.Username,
                    "",
                    ["root"]
                ),
                Password = contract.Password ?? RandomNumberGenerator.GetString(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                    16
                ),
                Host = _container.ContainerName,
            }
        };
    }

    public override Mysql Install(Mysql contract, ExecutionPlan plan)
    {
        Debug.Assert(_container.Installed);
        Debug.Assert(contract.Username != null);
        Debug.Assert(contract.Password != null);

        var network = plan.GetContract(contract.Network);
        _container.AttachNetwork(network);

        if (contract.Admin)
        {
            return contract;
        }

        Debug.Assert(contract.Database != null);

        RunSql(
            $"""
             CREATE DATABASE `{contract.Database}`;
             CREATE USER '{contract.Username}'@'%' IDENTIFIED BY '{contract.Password}';
             GRANT ALL PRIVILEGES ON `{contract.Database}`.* TO '{contract.Username}'@'%';
             FLUSH PRIVILEGES;
             """
        );

        return contract;
    }

    public override void Uninstall(Mysql contract)
    {
        Debug.Assert(contract.Installed);

        if (!contract.Admin)
        {
            RunSql(
                $"""
                 DROP DATABASE `{contract.Database}`;
                 DROP USER '{contract.Username}';
                 """
            );
        }

        var network = State.GetContract(contract.Network);
        _container.DetachNetwork(network);
    }

    /// <summary>
    /// Run sql with root privileges on the installed mysql server
    /// </summary>
    private void RunSql(string sql)
    {
        _container.ExecInContainer(["mysql", "-hlocalhost", "-uroot", $"-p{_rootPassword}", "-e", sql]);
    }
}