using System.Diagnostics;
using Frierun.Server.Data;
using Renci.SshNet;

namespace Frierun.Server.Handlers.Base;

public class SshConnectionHandler : Handler<SshConnection>
{
    public override IEnumerable<ContractInitializeResult> Initialize(SshConnection contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                Host = contract.Host ?? "",
                Port = contract.Port == 0 ? 22 : contract.Port,
                Username = contract.Username ?? "",
                Password = contract.Password ?? ""
            }
        );
    }

    public override SshConnection Install(SshConnection contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.Host != null);
        Debug.Assert(contract.Username != null);
        Debug.Assert(contract.Password != null);

        try
        {
            using var client = new SshClient(contract.Host, contract.Port, contract.Username, contract.Password);
            client.Connect();
        }
        catch (Exception)
        {
            throw new HandlerException(
                "Unable to connect to the server.",
                "Check if credentials are correct.",
                contract
            );
        }

        return contract;
    }
}