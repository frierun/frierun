using System.Diagnostics;
using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class MysqlHandler(Application application, State state)
    : Handler<Mysql>(application)
{
    private readonly Container _container = application.Contracts.OfType<Container>().Single();

    private readonly string _rootPassword = application.Contracts.OfType<Password>().Single().Value ??
                                            throw new Exception("Root password not found");

    public override IEnumerable<ContractInitializeResult> Initialize(Mysql contract, string prefix)
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
            
            yield return new ContractInitializeResult(
                contract with
                {
                    Username = "root",
                    Password = _rootPassword,
                    Handler = this,
                    DependsOn = contract.DependsOn.Append(contract.Network)
                }
            );
        }
        
        var baseName = contract.Database ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var database = baseName;
        while (state.Contracts.OfType<Mysql>().Any(c => c.Database == database))
        {
            count++;
            database = $"{baseName}{count}";
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                Database = database,
                Username = contract.Username ?? database,
                Password = contract.Password ?? RandomNumberGenerator.GetString(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                    16
                ),
                DependsOn = contract.DependsOn.Append(contract.Network)
            }
        );
    }

    public override Mysql Install(Mysql contract, ExecutionPlan plan)
    {
        Debug.Assert(_container.Installed);
        Debug.Assert(contract.Username != null);
        Debug.Assert(contract.Password != null);

        var network = plan.GetContract(contract.Network);
        Debug.Assert(network.Installed);
        
        if (contract.Host != null && contract.Host != _container.ContainerName)
        {
            throw new Exception("Host cannot be set");
        }
        if (contract.NetworkName != null && contract.NetworkName != network.NetworkName)
        {
            throw new Exception("NetworkName cannot be set");
        }
        
        _container.AttachNetwork(network.NetworkName);

        if (contract.Admin)
        {
            return contract with
            {
                Username = "root",
                Password = _rootPassword,
                Host = _container.ContainerName,
                NetworkName = network.NetworkName
            };
        }
        
        Debug.Assert(contract.Database != null);

        if (contract.Username == "root")
        {
            throw new Exception("Username cannot be root");
        }
        
        RunSql(
            $"""
             CREATE DATABASE `{contract.Database}`;
             CREATE USER '{contract.Username}'@'%' IDENTIFIED BY '{contract.Password}';
             GRANT ALL PRIVILEGES ON `{contract.Database}`.* TO '{contract.Username}'@'%';
             FLUSH PRIVILEGES;
             """
        );


        return contract with
        {
            Host = _container.ContainerName,
            NetworkName = network.NetworkName
        };
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

        _container.DetachNetwork(contract.NetworkName);
    }

    /// <summary>
    /// Run sql with root privileges on the installed mysql server
    /// </summary>
    private void RunSql(string sql)
    {
        _container.ExecInContainer(["mysql", "-u", "root", $"-p{_rootPassword}", "-e", sql]).Wait();
    }
}