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
        InstallPackage("static-domain");
        var providerApplication = InstallPackage("traefik");

        var container = Factory<Container>().Generate();
        List<Contract> contracts =
        [
            container,
            Factory<HttpEndpoint>().Generate() with { ContainerName = container.Name }
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
        InstallPackage("static-domain");
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        var endpointContract = application.Contracts.OfType<HttpEndpoint>().Single().Result;
        Assert.NotNull(endpointContract);
        Assert.Equal(80, endpointContract.Port);
        Assert.False(endpointContract.Ssl);
        Assert.StartsWith("http://", endpointContract.Url.ToString());
    }

    [Fact]
    public void Install_ExternalDomain_InstallHttpEndpoint()
    {
        InstallPackage(
            "static-domain",
            [new Selector("Internal", SelectedOption: "No")]
        );
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        var endpointContract = application.Contracts.OfType<HttpEndpoint>().Single().Result;
        Assert.NotNull(endpointContract);
        Assert.Equal(443, endpointContract.Port);
        Assert.True(endpointContract.Ssl);
        Assert.StartsWith("https://", endpointContract.Url.ToString());
    }

    [Fact]
    public void Install_NonDefaultPorts_SkipsSsl()
    {
        InstallPackage(
            "static-domain",
            [new Selector("Internal", SelectedOption: "No")]
        );
        InstallPackage(
            "traefik",
            [
                new PortEndpoint(Protocol.Tcp, 80, Name: "Web", DestinationPort: 81),
                new PortEndpoint(Protocol.Tcp, 443, Name: "WebSecure", DestinationPort: 444),
            ]
        );
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        var endpointContract = application.Contracts.OfType<HttpEndpoint>().Single().Result;
        Assert.NotNull(endpointContract);
        Assert.Equal(81, endpointContract.Port);
        Assert.False(endpointContract.Ssl);
        Assert.StartsWith("http://", endpointContract.Url.ToString());
    }

    [Fact]
    public void Install_PackageWithTwoContracts_OnlyOneNetworkAttached()
    {
        InstallPackage(
            "static-domain",
            [new Selector("Internal", SelectedOption: "No")]
        );
        InstallPackage("traefik");
        var package = Factory<Package>().Generate() with { Contracts = Factory<HttpEndpoint>().Generate(2) };
        
        var application = InstallPackage(package);

        var installedContracts = application.Contracts.OfType<HttpEndpoint>().ToList();
        Assert.Equal(2, installedContracts.Count);
        for (var i = 0; i < 2; i++)
        {
            var contractResult = installedContracts[i].Result;
            Assert.NotNull(contractResult);
            var traefikContractResult = (TraefikHttpEndpoint)contractResult;
            Assert.Equal(application.Name, traefikContractResult.NetworkName);
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
            "static-domain",
            [new Selector("Internal", SelectedOption: "No")]
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
}