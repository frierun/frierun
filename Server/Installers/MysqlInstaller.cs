using System.Diagnostics;
using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class MysqlInstaller(Application application, State state)
    : IHandler<Mysql>
{
    private readonly Container _container = application.Contracts.OfType<Container>().First();

    private readonly string _rootPassword = application.Contracts.OfType<Password>().First().Result?.Value ??
                                            throw new Exception("Root password not found");

    public Application Application => application;

    public IEnumerable<InstallerInitializeResult> Initialize(Mysql contract, string prefix)
    {
        var baseName = contract.DatabaseName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Contracts.OfType<Mysql>().Any(c => c.Result?.Database == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new InstallerInitializeResult(
            contract with
            {
                Handler = this,
                DatabaseName = name,
                DependsOn = contract.DependsOn.Append(contract.NetworkId)
            }
        );
    }

    public Mysql Install(Mysql contract, ExecutionPlan plan)
    {
        var network = plan.GetResource<DockerNetwork>(contract.NetworkId);
        _container.AttachNetwork(network.Name);

        if (contract.Admin)
        {
            return contract with
            {
                Result = new MysqlDatabase
                {
                    User = "root",
                    Password = _rootPassword,
                    Host = _container.Name,
                    NetworkName = network.Name
                }
            };
        }

        var name = contract.DatabaseName;
        var password = RandomNumberGenerator.GetString(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
            16
        );

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("Empty name");
        }

        RunSql(
            $"""
             CREATE DATABASE `{name}`;
             CREATE USER '{name}'@'%' IDENTIFIED BY '{password}';
             GRANT ALL PRIVILEGES ON `{name}`.* TO '{name}'@'%';
             FLUSH PRIVILEGES;
             """
        );


        return contract with
        {
            Result = new MysqlDatabase
            {
                User = name,
                Password = password,
                Database = name,
                Host = _container.Name,
                NetworkName = network.Name
            }
        };
    }

    void IHandler<Mysql>.Uninstall(Mysql contract)
    {
        var resource = contract.Result;
        Debug.Assert(resource != null);
        
        if (resource.User != "root")
        {
            RunSql(
                $"""
                 DROP DATABASE `{resource.Database}`;
                 DROP USER '{resource.User}';
                 """
            );
        }

        _container.DetachNetwork(resource.NetworkName);
    }

    /// <summary>
    /// Run sql with root privileges on the installed mysql server
    /// </summary>
    private void RunSql(string sql)
    {
        _container.ExecInContainer(["mysql", "-u", "root", $"-p{_rootPassword}", "-e", sql]).Wait();
    }
}