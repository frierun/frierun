using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class CloudflareTunnelHandler(State state) : Handler<CloudflareTunnel>(state)
{
    public override IEnumerable<ContractList> Initialize(CloudflareTunnel contract, ApplicationContext context)
    {
        if (contract.Container == null)
        {
            contract = contract with { Container = new ContractRef<Container>() };
        }
        
        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                TunnelName = contract.TunnelName ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    tunnel => tunnel.TunnelName
                )
            },
            [contract.Container] = new Container
            {
                ImageName = "cloudflare/cloudflared:latest",
                Command = new Argument<IEnumerable<string>>(plan =>
                    [
                        "tunnel",
                        "--no-autoupdate",
                        "run",
                        "--token",
                        plan.GetContract(new ContractRef<CloudflareTunnel>(context.Name)).Token ?? ""
                    ]
                ),
                DependsOn = [new ContractRef<CloudflareTunnel>(context.Name)]
            }
        };
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

        return contract with
        {
            TunnelId = tunnel.id,
            Token = tunnel.token
        };
    }

    public override void Uninstall(CloudflareTunnel contract)
    {
        Debug.Assert(contract.Installed);
        var cloudflareApiConnection = State.GetContract<CloudflareApiConnection>(contract.CloudflareApiConnection.Guid);

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