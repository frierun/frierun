using Frierun.Server.Data;
using Frierun.Server.Services;

namespace Frierun.Server.Installers;

public class TraefikHttpEndpointInstaller(DockerService dockerService, Application application)
    : IInstaller<HttpEndpoint>, IUninstaller<TraefikHttpEndpoint>
{
    /// <inheritdoc />
    InstallerInitializeResult IInstaller<HttpEndpoint>.Initialize(HttpEndpoint contract, string prefix, State state)
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

        return new InstallerInitializeResult(
            contract with { DomainName = name },
            [contract.ContainerId]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<HttpEndpoint>.GetDependencies(HttpEndpoint contract, ExecutionPlan plan)
    {
        var containerContract = plan.GetContract(contract.ContainerId);
        yield return new ContractDependency(contract.Id, contract.ContainerId);
        yield return new ContractDependency(containerContract.NetworkId, containerContract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<HttpEndpoint>.Install(HttpEndpoint contract, ExecutionPlan plan)
    {
        var domain = contract.DomainName!;
        var subdomain = domain.Split('.')[0];

        var containerContract = plan.GetContract(contract.ContainerId);

        var network = plan.GetResource<DockerNetwork>(containerContract.NetworkId);

        var traefikContainer = application.AllDependencies.OfType<DockerContainer>().FirstOrDefault();
        if (traefikContainer == null)
        {
            throw new Exception("Traefik container not found");
        }

        var traefikPort = application.AllDependencies.OfType<DockerPortEndpoint>().FirstOrDefault();
        if (traefikPort == null)
        {
            throw new Exception("Traefik port not found");
        }

        var resource = new TraefikHttpEndpoint(domain, traefikPort.Port)
        {
            DependsOn =
            [
                application,
                network
            ]
        };

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

        dockerService.AttachNetwork(network.Name, traefikContainer.Name).Wait();
        return resource;
    }

    /// <inheritdoc />
    void IUninstaller<TraefikHttpEndpoint>.Uninstall(TraefikHttpEndpoint resource)
    {
        var network = resource.DependsOn.OfType<DockerNetwork>().FirstOrDefault();
        if (network == null)
        {
            return;
        }

        var container = application.AllDependencies.OfType<DockerContainer>().First();
        dockerService.DetachNetwork(network.Name, container.Name).Wait();
    }
}