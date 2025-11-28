using System.Diagnostics;
using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class PostgresqlHandler(State state, Application application, ILogger<PostgresqlHandler> logger)
    : Handler<Postgresql>(state, application)
{
    private readonly Container _container = state.GetContract<Container>(application);

    private readonly string _rootPassword = state.GetContract<Password>(application).Value ??
                                            throw new Exception("Root password not found");

    public override IEnumerable<ContractList> Initialize(Postgresql contract, ApplicationContext context)
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

            yield return new ContractList
            {
                [context] = contract with
                {
                    Handler = this,
                    Username = "postgres",
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
                    c => c.Database
                ),
                Username = contract.Username ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.Username,
                    "",
                    ["postgres"]
                ),
                Password = contract.Password ?? RandomNumberGenerator.GetString(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                    16
                ),
                Host = _container.ContainerName,
            }
        };
    }

    public override Postgresql Install(Postgresql contract, ExecutionPlan plan)
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
            [
                $"CREATE DATABASE \"{contract.Database}\"",
                $"CREATE USER \"{contract.Username}\" WITH ENCRYPTED PASSWORD '{contract.Password}'",
                $"ALTER DATABASE \"{contract.Database}\" OWNER TO \"{contract.Username}\""
            ]
        );
        
        return contract;
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

        var network = State.GetContract(contract.Network);
        _container.DetachNetwork(network);
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