using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class CloudflareTunnelHandler : Handler<CloudflareTunnel>
{
    public override IEnumerable<ContractInitializeResult> Initialize(CloudflareTunnel contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                TunnelName = contract.TunnelName ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    tunnel => tunnel.TunnelName
                ),
                DependsOn = [contract.CloudflareApiConnection],
                DependencyOf = [contract.Container],
            },
            [
                new Container(
                    Name: contract.Container.Name,
                    ImageName: "cloudflare/cloudflared:latest"
                )
            ]
        );
    }

    public override CloudflareTunnel Install(CloudflareTunnel contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.TunnelName != null);
        var cloudflareApiConnection = plan.GetContract(contract.CloudflareApiConnection);

        var client = cloudflareApiConnection.CreateClient();
        IEnumerable<(string id, string name)> accounts;
        try
        {
            accounts = client.GetAccounts();
        }
        catch (Exception e)
        {
            throw new HandlerException(
                "Failed to get accounts from Cloudflare API.",
                "Check your Cloudflare API token and permissions.",
                contract,
                e
            );
        }

        if (contract.AccountId == null)
        {
            var account = accounts.FirstOrDefault();
            if (account == default)
            {
                throw new HandlerException(
                    "No Cloudflare accounts found.",
                    "Ensure you have at least one account in your Cloudflare settings.",
                    contract
                );
            }

            contract = contract with { AccountId = account.id };
        }
        else
        {
            if (accounts.All(a => a.id != contract.AccountId))
            {
                throw new HandlerException(
                    $"Account with ID {contract.AccountId} not found.",
                    "Check the account ID in your Cloudflare settings.",
                    contract
                );
            }
        }

        (string id, string token) tunnel;
        try
        {
            tunnel = client.CreateTunnel(contract.AccountId, contract.TunnelName);
        }
        catch (Exception e)
        {
            throw new HandlerException(
                "Failed to create Cloudflare Tunnel.",
                "Check your Cloudflare API token and permissions.",
                contract,
                e
            );
        }

        var container = plan.GetContract(contract.Container);
        plan.ReplaceContract(
            container with
            {
                Command = ["tunnel", "--no-autoupdate", "run", "--token", tunnel.token]
            }
        );

        return contract with
        {
            TunnelId = tunnel.id,
            Token = tunnel.token
        };
    }

    public override void Uninstall(CloudflareTunnel contract)
    {
        Debug.Assert(contract.Installed);
        var application = State.Applications.Single(application => application.Contracts.Contains(contract));
        var cloudflareApiConnection = application.GetContract(contract.CloudflareApiConnection);

        var client = cloudflareApiConnection.CreateClient();
        try
        {
            client.DeleteTunnel(contract.AccountId, contract.TunnelId);
        }
        catch (Exception e)
        {
            throw new HandlerException(
                "Failed to delete Cloudflare Tunnel.",
                "Check your Cloudflare API token and permissions.",
                contract,
                e
            );
        }
    }
}