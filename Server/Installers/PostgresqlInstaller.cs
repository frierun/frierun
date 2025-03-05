﻿using System.Security.Cryptography;
using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server.Installers;

public class PostgresqlInstaller(DockerService dockerService, Application application)
    : IInstaller<Postgresql>, IUninstaller<PostgresqlDatabase>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Postgresql>.Initialize(Postgresql contract, string prefix, State state)
    {
        var baseName = contract.DatabaseName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<PostgresqlDatabase>().Any(c => c.Database == name))
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
    IEnumerable<ContractDependency> IInstaller<Postgresql>.GetDependencies(Postgresql contract, ExecutionPlan plan)
    {
        yield return new ContractDependency(contract.NetworkId, contract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<Postgresql>.Install(Postgresql contract, ExecutionPlan plan)
    {
        var network = plan.GetResource<DockerNetwork>(contract.NetworkId);

        var container = application.DependsOn.OfType<DockerContainer>().First();
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
             CREATE DATABASE "{name}";
             CREATE USER "{name}" WITH ENCRYPTED PASSWORD '{password}';
             GRANT ALL PRIVILEGES ON DATABASE "{name}" TO "{name}";
             """
        );

        dockerService.AttachNetwork(network.Name, container.Name).Wait();

        return new PostgresqlDatabase(name, password, name, container.Name)
        {
            DependsOn =
            [
                application,
                network
            ]
        };
    }

    /// <inheritdoc />
    void IUninstaller<PostgresqlDatabase>.Uninstall(PostgresqlDatabase resource)
    {
        RunSql(
            $"""
             DROP DATABASE "{resource.Database}";
             DROP USER "{resource.User}";
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
    /// Run sql with admin privileges on the installed postgresql server
    /// </summary>
    private void RunSql(string sql)
    {
        var container = application.DependsOn.OfType<DockerContainer>().First();

        dockerService.ExecInContainer(
            container.Name,
            ["psql", "-U", "postgres", "-c", sql]
        ).Wait();
    }
}