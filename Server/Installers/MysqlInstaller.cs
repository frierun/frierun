﻿using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class MysqlInstaller(DockerService dockerService, Application application)
    : IInstaller<Mysql>, IUninstaller<MysqlDatabase>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Mysql>.Initialize(Mysql contract, string prefix, State state)
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
        var container = application.DependsOn.OfType<DockerContainer>().First();
        dockerService.AttachNetwork(network.Name, container.Name).Wait();
        
        if (contract.Admin)
        {
            return new MysqlDatabase(
                User: "root",
                Password: application.DependsOn.OfType<GeneratedPassword>().First().Value, 
                Database:"", 
                Host: container.Name
                )
            {
                DependsOn =
                [
                    application,
                    network
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


        return new MysqlDatabase(name, password, name, container.Name)
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
        if (resource.User != "root")
        {
            RunSql(
                $"""
                 DROP DATABASE `{resource.Database}`;
                 DROP USER '{resource.User}';
                 """
            );
        }

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
        var container = application.DependsOn.OfType<DockerContainer>().First();
        var rootPassword = application.DependsOn.OfType<GeneratedPassword>().First().Value;

        dockerService.ExecInContainer(
            container.Name,
            ["mysql", "-u", "root", $"-p{rootPassword}", "-e", sql]
        ).Wait();
    }
}