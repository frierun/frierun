using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class MysqlInstaller(DockerService dockerService, State state, Application application)
    : IInstaller<Mysql>, IUninstaller<MysqlDatabase>
{
    private readonly DockerContainer _container = application.DependsOn.OfType<DockerContainer>().First();
    private readonly string _rootPassword = application.DependsOn.OfType<GeneratedPassword>().First().Value;

    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Mysql>.Initialize(Mysql contract, string prefix)
    {
        var baseName = contract.DatabaseName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<MysqlDatabase>().Any(c => c.Database == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new InstallerInitializeResult(
            contract with { DatabaseName = name },
            null,
            [new ConnectExternalContainer(_container.Name, contract.NetworkName)]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Mysql>.GetDependencies(Mysql contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(
            new ConnectExternalContainer(_container.Name, contract.NetworkName),
            contract
        );
    }

    /// <inheritdoc />
    Resource IInstaller<Mysql>.Install(Mysql contract, ExecutionPlan plan)
    {
        if (contract.Admin)
        {
            return new MysqlDatabase(
                User: "root",
                Password: _rootPassword,
                Database: "",
                Host: _container.Name
            )
            {
                DependsOn =
                [
                    application,
                ]
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


        return new MysqlDatabase(name, password, name, _container.Name)
        {
            DependsOn =
            [
                application,
            ]
        };
    }

    /// <inheritdoc />
    void IUninstaller<MysqlDatabase>.Uninstall(MysqlDatabase resource)
    {
        if (resource.User != "root")
        {
            RunSql(
                $"""
                 DROP DATABASE `{resource.Database}`;
                 DROP USER '{resource.User}';
                 """
            );
        }
    }

    /// <summary>
    /// Run sql with root privileges on the installed mysql server
    /// </summary>
    private void RunSql(string sql)
    {
        dockerService.ExecInContainer(
            _container.Name,
            ["mysql", "-u", "root", $"-p{_rootPassword}", "-e", sql]
        ).Wait();
    }
}