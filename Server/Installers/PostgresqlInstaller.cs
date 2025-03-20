﻿using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class PostgresqlInstaller(
    DockerService dockerService,
    State state,
    Application application,
    ILogger<PostgresqlInstaller> logger)
    : IInstaller<Postgresql>, IUninstaller<PostgresqlDatabase>
{
    private readonly DockerContainer _container = application.DependsOn.OfType<DockerContainer>().First();
    
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Postgresql>.Initialize(
        Postgresql contract,
        string prefix
    )
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
            null,
            [new ConnectExternalContainer(_container.Name, contract.NetworkName)]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Postgresql>.GetDependencies(Postgresql contract, ExecutionPlan plan)
    {
        var attachNetworkContract = new ConnectExternalContainer(_container.Name, contract.NetworkName);
        yield return new ContractDependency(attachNetworkContract.Id, contract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<Postgresql>.Install(Postgresql contract, ExecutionPlan plan)
    {
        var attachNetworkContract = new ConnectExternalContainer(_container.Name, contract.NetworkName);
        var attachedNetwork = plan.GetResource<DockerAttachedNetwork>(attachNetworkContract.Id);

        if (contract.Admin)
        {
            return new PostgresqlDatabase(
                User: "postgres",
                Password: application.DependsOn.OfType<GeneratedPassword>().First().Value,
                Database: "",
                Host: _container.Name
            )
            {
                DependsOn =
                [
                    application,
                    attachedNetwork
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
            [
                $"CREATE DATABASE \"{name}\"",
                $"CREATE USER \"{name}\" WITH ENCRYPTED PASSWORD '{password}'",
                $"ALTER DATABASE \"{name}\" OWNER TO \"{name}\""
            ]
        );


        return new PostgresqlDatabase(name, password, name, _container.Name)
        {
            DependsOn =
            [
                application,
                attachedNetwork
            ]
        };
    }

    /// <inheritdoc />
    void IUninstaller<PostgresqlDatabase>.Uninstall(PostgresqlDatabase resource)
    {
        if (resource.User != "postgres")
        {
            RunSql(
                [
                    $"DROP DATABASE \"{resource.Database}\"",
                    $"DROP USER \"{resource.User}\""
                ]
            );
        }
    }

    /// <summary>
    /// Run sql with admin privileges on the installed postgresql server
    /// </summary>
    private void RunSql(IList<string> sqlList)
    {
        var command = new List<string> { "psql", "-U", "postgres" };
        command.AddRange(sqlList.SelectMany(sql => new[] { "-c", sql }));

        var result = dockerService.ExecInContainer(
            _container.Name,
            command
        ).Result;

        logger.LogDebug(
            "Executed sql: {Sql}\nStdout: {Stdout}\nStderr: {Stderr}",
            sqlList,
            result.stdout,
            result.stderr
        );
    }
}