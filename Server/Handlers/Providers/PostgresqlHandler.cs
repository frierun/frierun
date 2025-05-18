using System.Diagnostics;
using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class PostgresqlHandler(
    Application application,
    State state,
    ILogger<PostgresqlHandler> logger)
    : IHandler<Postgresql>
{
    private readonly Container _container = application.Contracts.OfType<Container>().First();

    private readonly string _rootPassword = application.Contracts.OfType<Password>().First().Result?.Value ??
                                            throw new Exception("Root password not found");
    
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(
        Postgresql contract,
        string prefix
    )
    {
        var baseName = contract.DatabaseName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Contracts.OfType<Postgresql>().Any(c => c.Result?.Database == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                DatabaseName = name,
                DependsOn = contract.DependsOn.Append(contract.NetworkId)
            }
        );
    }

    public Postgresql Install(Postgresql contract, ExecutionPlan plan)
    {
        var network = plan.GetResource<DockerNetwork>(contract.NetworkId);

        _container.AttachNetwork(network.Name);
        Debug.Assert(_container.Result != null);

        if (contract.Admin)
        {
            return contract with
            {
                Result = new PostgresqlDatabase
                {
                    User = "postgres",
                    Password = _rootPassword,
                    Host = _container.Result.Name,
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
            [
                $"CREATE DATABASE \"{name}\"",
                $"CREATE USER \"{name}\" WITH ENCRYPTED PASSWORD '{password}'",
                $"ALTER DATABASE \"{name}\" OWNER TO \"{name}\""
            ]
        );

        return contract with
        {
            Result = new PostgresqlDatabase
            {
                User = name,
                Password = password,
                Database = name,
                Host = _container.Result.Name,
                NetworkName = network.Name
            }
        };
    }

    void IHandler<Postgresql>.Uninstall(Postgresql contract)
    {
        var resource = contract.Result;
        Debug.Assert(resource != null);
        
        if (resource.User != "postgres")
        {
            RunSql(
                [
                    $"DROP DATABASE \"{resource.Database}\"",
                    $"DROP USER \"{resource.User}\""
                ]
            );
        }

        _container.DetachNetwork(resource.NetworkName);
    }

    /// <summary>
    /// Run sql with admin privileges on the installed postgresql server
    /// </summary>
    private void RunSql(IList<string> sqlList)
    {
        var command = new List<string> { "psql", "-U", "postgres" };
        command.AddRange(sqlList.SelectMany(sql => new[] { "-c", sql }));

        var result = _container.ExecInContainer(command).Result;

        logger.LogDebug(
            "Executed sql: {Sql}\nStdout: {Stdout}\nStderr: {Stderr}",
            sqlList,
            result.stdout,
            result.stderr
        );
    }
}