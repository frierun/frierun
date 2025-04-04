﻿using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests.Installers;

public class TraefikHttpEndpointInstallerTests : BaseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Install_ContainerWithHttpEndpoint_InstallTraefikFirst(bool reverseOrder)
    {
        InstallPackage("static-domain");
        var providerApplication = InstallPackage("traefik");
        Assert.NotNull(providerApplication);

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

        Assert.NotNull(application);
        var resources = application.Resources.ToList();
        var endpointIndex = resources.FindIndex(r => r is TraefikHttpEndpoint);
        var containerIndex = resources.FindIndex(r => r is DockerContainer);
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

        Assert.NotNull(application);
        var endpointContract = application.Resources.OfType<TraefikHttpEndpoint>().First();
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

        Assert.NotNull(application);
        var endpointContract = application.Resources.OfType<TraefikHttpEndpoint>().First();
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
                new PortEndpoint( Protocol.Tcp, 80, Name: "Web", DestinationPort: 81),
                new PortEndpoint( Protocol.Tcp, 443, Name: "WebSecure", DestinationPort: 444),
            ]
        );
        var package = Factory<Package>().Generate() with { Contracts = [Factory<HttpEndpoint>().Generate()] };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var endpointContract = application.Resources.OfType<TraefikHttpEndpoint>().First();
        Assert.Equal(81, endpointContract.Port);
        Assert.False(endpointContract.Ssl);
        Assert.StartsWith("http://", endpointContract.Url.ToString());
    }
}