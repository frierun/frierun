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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithHttpEndpoint_InstallTraefikFirst(bool reverseOrder)
    {
        InstallPackage("static-zone");
        InstallPackage("traefik");

        var container = Factory<Container>().Generate();
        List<Contract> contracts =
        [
            container,
            Factory<HttpEndpoint>().Generate() with { Container = (ContractId<Container>)container.Id }
        ];
        if (reverseOrder)
        {
            contracts.Reverse();
        }

        var package = Factory<Package>().Generate() with { Contracts = contracts };

        var application = InstallPackage(package);

        var installedContracts = application.Contracts.ToList();
        var endpointIndex = installedContracts.FindIndex(r => r is HttpEndpoint);
        var containerIndex = installedContracts.FindIndex(r => r is Container);
        Assert.NotEqual(-1, endpointIndex);
        Assert.NotEqual(-1, containerIndex);
        Assert.True(endpointIndex < containerIndex);
    }

    [Fact]
    public void Install_InternalDomain_InstallHttpEndpoint()
    {
        InstallPackage("static-zone");
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        var endpointContract = application.Contracts.OfType<HttpEndpoint>().Single();
        Assert.True(endpointContract.Installed);
        Assert.Equal(80, endpointContract.Url.Port);
        Assert.Equal("http", endpointContract.Url.Scheme);
        Assert.StartsWith("http://", endpointContract.Url.ToString());
    }

    [Fact]
    public void Install_ExternalDomain_InstallHttpEndpoint()
    {
        InstallPackage(
            "static-zone",
            [new Selector("Internal", Value: "No")]
        );
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        var endpointContract = application.Contracts.OfType<HttpEndpoint>().Single();
        Assert.True(endpointContract.Installed);
        Assert.Equal(443, endpointContract.Url.Port);
        Assert.Equal("https", endpointContract.Url.Scheme);
        Assert.StartsWith("https://", endpointContract.Url.ToString());
    }

    [Fact]
    public void Install_NonDefaultPorts_SkipsSsl()
    {
        InstallPackage(
            "static-zone",
            [new Selector("Internal", Value: "No")]
        );
        InstallPackage(
            "traefik",
            [
                new PortEndpoint(Protocol.Tcp, 80, Name: "Web", ExternalPort: 81),
                new PortEndpoint(Protocol.Tcp, 443, Name: "WebSecure", ExternalPort: 444),
            ]
        );
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        var endpointContract = application.Contracts.OfType<HttpEndpoint>().Single();
        Assert.True(endpointContract.Installed);
        Assert.Equal(81, endpointContract.Url.Port);
        Assert.Equal("http", endpointContract.Url.Scheme);
        Assert.StartsWith("http://", endpointContract.Url.ToString());
    }

    [Fact]
    public void Install_PackageWithTwoContracts_OnlyOneNetworkAttached()
    {
        InstallPackage(
            "static-zone",
            [new Selector("Internal", Value: "No")]
        );
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = Factory<HttpEndpoint>().Generate(2) };
        
        var application = InstallPackage(package);

        var installedContracts = application.Contracts.OfType<HttpEndpoint>().ToList();
        Assert.Equal(2, installedContracts.Count);
        for (var i = 0; i < 2; i++)
        {
            var contract = installedContracts[i];
            Assert.True(contract.Installed);
            Assert.Equal(application.Name, contract.NetworkName);
        }
        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkConnectParameters>()
        );
    }

    [Fact]
    public void Uninstall_PackageWithTwoContracts_OnlyOneNetworkDetached()
    {
        InstallPackage(
            "static-zone",
            [new Selector("Internal", Value: "No")]
        );
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = Factory<HttpEndpoint>().Generate(2) };
        var application = InstallPackage(package);

        Resolve<UninstallService>().Handle(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            application.Name,
            Arg.Any<NetworkDisconnectParameters>()
        );
    }
    
    [Fact]
    public void Install_HttpEndpoint_ContainerIsLabeled()
    {
        InstallPackage("static-zone");
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };
        
        var application = InstallPackage(package);

        var installedContracts = application.Contracts.OfType<HttpEndpoint>().ToList();
        Assert.Single(installedContracts);
        
        DockerClient.Containers.Received(1).CreateContainerAsync(
            Arg.Is<CreateContainerParameters>(p => p.Labels.ContainsKey("traefik.enable")),
            Arg.Any<CancellationToken>()
        );
    }    
}