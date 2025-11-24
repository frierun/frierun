using Docker.DotNet.Models;
using Frierun.Server;
using Frierun.Server.Data;
using NSubstitute;

namespace Frierun.Tests.Handlers;

public class TraefikHttpEndpointHandlerTests : BaseTests
{
    public TraefikHttpEndpointHandlerTests()
    {
        InstallPackage("docker");
    }

    [Fact]
    public void Install_ContainerWithHttpEndpoint_DependsOnContainer()
    {
        InstallPackage("static-zone");
        InstallPackage("traefik");

        var container = Contract<Container>().Generate();
        var httpEndpoint = Contract<HttpEndpoint>().Set(p => p.Container, container.Ref).Generate();
        var package = Factory<Package>().Generate() with { Contracts = [container, httpEndpoint] };

        var application = InstallPackage(package);

        Assert.Contains(
            State.GetContract(application, container.Ref).Id,
            State.GetContract(application, httpEndpoint.Ref).DependsOn);
    }

    [Fact]
    public void Install_InternalDomain_InstallsHttpEndpoint()
    {
        InstallPackage("static-zone");
        InstallPackage("traefik");
        var httpEndpoint = Contract<HttpEndpoint>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [httpEndpoint] };

        var application = InstallPackage(package);

        var installedHttpEndpoint = State.GetContract(application, httpEndpoint.Ref);
        Assert.True(installedHttpEndpoint.Installed);
        Assert.Equal(80, installedHttpEndpoint.Url.Value.Port);
        Assert.Equal("http", installedHttpEndpoint.Url.Value.Scheme);
        Assert.StartsWith("http://", installedHttpEndpoint.Url.ToString());
    }

    [Fact]
    public void Install_ExternalDomain_InstallsHttpEndpoint()
    {
        InstallPackage(
            "static-zone",
            new ContractList { ["Internal"] = new Selector(Value: "No") }
        );
        InstallPackage("traefik");
        var httpEndpoint = Contract<HttpEndpoint>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [httpEndpoint] };

        var application = InstallPackage(package);

        var installedHttpEndpoint = State.GetContract(application, httpEndpoint.Ref);
        Assert.True(installedHttpEndpoint.Installed);
        Assert.Equal(443, installedHttpEndpoint.Url.Value.Port);
        Assert.Equal("https", installedHttpEndpoint.Url.Value.Scheme);
        Assert.StartsWith("https://", installedHttpEndpoint.Url.ToString());
    }

    [Fact]
    public void Install_NonDefaultPorts_SkipsSsl()
    {
        InstallPackage(
            "static-zone",
            new ContractList
            {
                ["Internal"] = new Selector(Value: "No")
            }
        );
        InstallPackage(
            "traefik",
            new ContractList
            {
                ["Web"] = new PortEndpoint(Protocol.Tcp, 80, ExternalPort: 81),
                ["WebSecure"] = new PortEndpoint(Protocol.Tcp, 443, ExternalPort: 444),
            }
        );
        var httpEndpoint = Contract<HttpEndpoint>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [httpEndpoint] };

        var application = InstallPackage(package);

        var installedHttpEndpoint = State.GetContract(application, httpEndpoint.Ref);
        Assert.True(installedHttpEndpoint.Installed);
        Assert.Equal(81, installedHttpEndpoint.Url.Value.Port);
        Assert.Equal("http", installedHttpEndpoint.Url.Value.Scheme);
        Assert.StartsWith("http://", installedHttpEndpoint.Url.ToString());
    }

    [Fact]
    public void Install_PackageWithTwoContracts_AttachesOnlyOneNetwork()
    {
        InstallPackage(
            "static-zone",
            new ContractList { ["Internal"] = new Selector(Value: "No") }
        );
        InstallPackage("traefik");
        var httpEndpoint1 = Contract<HttpEndpoint>().Generate();
        var httpEndpoint2 = Contract<HttpEndpoint>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [httpEndpoint1, httpEndpoint2] };

        var application = InstallPackage(package);

        var installedHttpEndpoint1 = State.GetContract(application, httpEndpoint1.Ref);
        var installedHttpEndpoint2 = State.GetContract(application, httpEndpoint2.Ref);
        Assert.True(installedHttpEndpoint1.Installed);
        Assert.True(installedHttpEndpoint2.Installed);
        Assert.Equal(application.Name, installedHttpEndpoint1.NetworkName);
        Assert.Equal(application.Name, installedHttpEndpoint2.NetworkName);

        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkConnectParameters>()
        );
    }

    [Fact]
    public void Uninstall_PackageWithTwoContracts_DetachesOnlyOneNetwork()
    {
        InstallPackage(
            "static-zone",
            new ContractList { ["Internal"] = new Selector(Value: "No") }
        );
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                Contract<HttpEndpoint>().Generate(),
                Contract<HttpEndpoint>().Generate()
            ]
        };
        var application = InstallPackage(package);

        Resolve<UninstallService>().Handle(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkDisconnectParameters>()
        );
    }

    [Fact]
    public void Install_HttpEndpoint_AddsContainerLabel()
    {
        InstallPackage("static-zone");
        InstallPackage("traefik");
        var httpEndpoint = Contract<HttpEndpoint>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [httpEndpoint] };

        var application = InstallPackage(package);

        var contract = State.GetContract(application, httpEndpoint.Ref);
        var router = contract.TraefikRouterName;
        var host = $"Host(`{contract.ResultHost.Value}`)";
        Assert.NotNull(router);

        DockerClient.Containers.Received(1).CreateContainerAsync(
            Arg.Is<CreateContainerParameters>(p =>
                p.Labels["traefik.enable"] == "true"
                && p.Labels[$"traefik.http.routers.{router}.rule"] == host
                && p.Labels[$"traefik.http.services.{router}.loadbalancer.server.port"] == contract.Port.ToString()
                && p.Labels[$"traefik.http.routers.{router}.tls"] == "false"
                && !p.Labels.ContainsKey($"traefik.http.routers.{router}.tls.certresolver")
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public void Install_HttpsEndpoint_AddsContainerLabel()
    {
        InstallPackage(
            "static-zone",
            new ContractList { ["Internal"] = new Selector(Value: "No") }
        );
        InstallPackage("traefik");
        var httpEndpoint = Contract<HttpEndpoint>().Generate();
        var package = Factory<Package>().Generate() with { Contracts = [httpEndpoint] };

        var application = InstallPackage(package);

        var contract = State.GetContract(application, httpEndpoint.Ref);
        var router = contract.TraefikRouterName;
        var host = $"Host(`{contract.ResultHost.Value}`)";
        Assert.NotNull(router);

        DockerClient.Containers.Received(1).CreateContainerAsync(
            Arg.Is<CreateContainerParameters>(p =>
                p.Labels["traefik.enable"] == "true"
                && p.Labels[$"traefik.http.routers.{router}.rule"] == host
                && p.Labels[$"traefik.http.services.{router}.loadbalancer.server.port"] == contract.Port.ToString()
                && p.Labels[$"traefik.http.routers.{router}.tls"] == "true"
                && p.Labels[$"traefik.http.routers.{router}.tls.certresolver"] == "httpchallenge"
            ),
            Arg.Any<CancellationToken>()
        );
    }
}