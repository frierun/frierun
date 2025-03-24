﻿using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class TraefikHttpEndpointInstaller(State state, Application application)
    : IInstaller<HttpEndpoint>, IUninstaller<TraefikHttpEndpoint>
{
    private readonly DockerContainer _container = application.Resources.OfType<DockerContainer>().First();
    private readonly DockerPortEndpoint _port = application.Resources.OfType<DockerPortEndpoint>().First();

    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix)
    {
        var baseName = contract.DomainName ?? $"{prefix}.localhost";
        var subdomain = baseName.Split('.')[0];
        var domain = baseName.Substring(subdomain.Length + 1);

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<TraefikHttpEndpoint>().Any(c => c.Domain == name))
        {
            count++;
            name = $"{subdomain}{count}.{domain}";
        }

        var connectExternalContainer = new ConnectExternalContainer(_container.Name);        
        yield return new InstallerInitializeResult(
            contract with
            {
                DomainName = name,
                DependsOn = contract.DependsOn.Append(connectExternalContainer),
                DependencyOf = contract.DependencyOf.Append(contract.ContainerId),
            },
            [contract.ContainerId],
            [connectExternalContainer]
        );
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domain = contract.DomainName!;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);

        plan.UpdateContract(
            containerContract with
            {
                Configure = containerContract.Configure.Append(
                    parameters =>
                    {
                        parameters.Labels["traefik.enable"] = "true";
                        parameters.Labels[$"traefik.http.routers.{subdomain}.rule"] = $"Host(`{domain}`)";
                        parameters.Labels[$"traefik.http.services.{subdomain}.loadbalancer.server.port"] =
                            contract.Port.ToString();
                    }
                ),
            }
        );

        plan.RequireApplication(application);
        return new TraefikHttpEndpoint(domain, _port.Port);
    }
}