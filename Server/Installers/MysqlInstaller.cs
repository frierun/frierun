using System.Security.Cryptography;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server.Installers;

public class MysqlInstaller(DockerService dockerService, Application application)
    : IInstaller<Mysql>, IUninstaller<MysqlDatabase>
{
    /// <inheritdoc />
    InstallerInitializeResult IInstaller<Mysql>.Initialize(Mysql contract, string prefix, State state)
    {
        var baseName = contract.DatabaseName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<MysqlDatabase>().Any(c => c.Database == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        return new InstallerInitializeResult(
            contract with { DatabaseName = name },
            [contract.NetworkId]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Mysql>.GetDependencies(Mysql contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.NetworkId, contract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<Mysql>.Install(Mysql contract, ExecutionPlan plan)
    {
        var network = plan.GetResource<DockerNetwork>(contract.NetworkId);

        var mysqlContainer = application.DependsOn.OfType<DockerContainer>().First();
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
             CREATE DATABASE {name};
             CREATE USER '{name}'@'%' IDENTIFIED BY '{password}';
             GRANT ALL PRIVILEGES ON {name}.* TO '{name}'@'%';
             FLUSH PRIVILEGES;
             """
        );

        dockerService.AttachNetwork(network.Name, mysqlContainer.Name).Wait();

        return new MysqlDatabase(name, password, name, mysqlContainer.Name)
        {
            DependsOn =
            [
                application,
                network
            ]
        };
    }

    /// <inheritdoc />
    void IUninstaller<MysqlDatabase>.Uninstall(MysqlDatabase resource)
    {
        RunSql(
            $"""
             DROP DATABASE {resource.Database};
             DROP USER {resource.User};
             """
        );

        var network = resource.DependsOn.OfType<DockerNetwork>().FirstOrDefault();
        if (network == null)
        {
            return;
        }

        var container = application.AllDependencies.OfType<DockerContainer>().First();
        dockerService.DetachNetwork(network.Name, container.Name).Wait();
    }

    /// <summary>
    /// Run sql with root privileges on the installed mysql server
    /// </summary>
    private void RunSql(string sql)
    {
        var mysqlContainer = application.DependsOn.OfType<DockerContainer>().First();
        var rootPassword = application.DependsOn.OfType<GeneratedPassword>().First().Value;

        dockerService.ExecInContainer(
            mysqlContainer.Name,
            ["mysql", "-u", "root", $"-p{rootPassword}", "-e", sql]
        ).Wait();
    }
}