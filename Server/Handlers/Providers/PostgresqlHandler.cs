using System.Diagnostics;
using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class PostgresqlHandler(Application application, ILogger<PostgresqlHandler> logger)
    : Handler<Postgresql>(application)
{
    private readonly Container _container = application.Contracts.OfType<Container>().Single();

    private readonly string _rootPassword = application.Contracts.OfType<Password>().Single().Value ??
                                            throw new Exception("Root password not found");

    public override IEnumerable<ContractInitializeResult> Initialize(
        Postgresql contract,
        string prefix
    )
    {
        if (contract.Admin)
        {
            if (contract.Username != null && contract.Username != "postgres")
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
                    Handler = this,
                    Username = "postgres",
                    Password = _rootPassword,
                    Host = _container.ContainerName,
                    DependsOn = contract.DependsOn.Append(contract.Network)
                }
            );
        }

        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                Database = contract.Database ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    c => c.Database
                ),
                Username = contract.Username ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    c => c.Username,
                    "",
                    ["postgres"]
                ),
                Password = contract.Password ?? RandomNumberGenerator.GetString(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                    16
                ),
                Host = _container.ContainerName,
                DependsOn = contract.DependsOn.Append(contract.Network)
            }
        );
    }

    public override Postgresql Install(Postgresql contract, ExecutionPlan plan)
    {
        Debug.Assert(_container.Installed);
        Debug.Assert(contract.Username != null);
        Debug.Assert(contract.Password != null);

        var network = plan.GetContract(contract.Network);
        Debug.Assert(network.Installed);
        
        if (contract.NetworkName != null && contract.NetworkName != network.NetworkName)
        {
            throw new Exception("NetworkName cannot be set");
        }

        _container.AttachNetwork(network.NetworkName);

        if (contract.Admin)
        {
            return contract with
            {
                NetworkName = network.NetworkName
            };
        }

        Debug.Assert(contract.Database != null);

        RunSql(
            [
                $"CREATE DATABASE \"{contract.Database}\"",
                $"CREATE USER \"{contract.Username}\" WITH ENCRYPTED PASSWORD '{contract.Password}'",
                $"ALTER DATABASE \"{contract.Database}\" OWNER TO \"{contract.Username}\""
            ]
        );

        return contract with
        {
            NetworkName = network.NetworkName
        };
    }

    public override void Uninstall(Postgresql contract)
    {
        Debug.Assert(contract.Installed);

        if (!contract.Admin)
        {
            RunSql(
                [
                    $"DROP DATABASE \"{contract.Database}\"",
                    $"DROP USER \"{contract.Username}\""
                ]
            );
        }

        _container.DetachNetwork(contract.NetworkName);
    }

    /// <summary>
    /// Runs the SQL query with admin privileges on the installed postgresql server
    /// </summary>
    private void RunSql(IList<string> sqlList)
    {
        var command = new List<string> { "psql", "-U", "postgres" };
        command.AddRange(sqlList.SelectMany(sql => new[] { "-c", sql }));

        var result = _container.ExecInContainer(command);

        logger.LogDebug(
            "Executed sql: {Sql}\nStdout: {Stdout}\nStderr: {Stderr}",
            sqlList,
            result.stdout,
            result.stderr
        );
    }
}