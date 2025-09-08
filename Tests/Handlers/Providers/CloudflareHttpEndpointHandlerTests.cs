using System.Text.Json.Nodes;
using Docker.DotNet.Models;
using Frierun.Server.Data;
using Frierun.Server.Handlers;
using NSubstitute;

namespace Frierun.Tests.Handlers;

public class CloudflareHttpEndpointHandlerTests : BaseTests
{
    [Fact]
    public void Install_Frierun_Success()
    {
        InstallPackage("docker");
        var tunnelApplication = InstallPackage("cloudflare-tunnel");
        var tunnel = tunnelApplication.GetContracts<CloudflareTunnel>().Single();
        Assert.NotNull(tunnel.TunnelId);
        Assert.NotNull(tunnel.AccountId);

        var application = InstallPackage("frierun");

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        Assert.NotNull(httpEndpoint.CloudflareZoneId);
        Assert.NotNull(httpEndpoint.NetworkName);

        CloudflareClient.Received(1)
            .UpdateTunnelConfiguration(tunnel.AccountId, tunnel.TunnelId, Arg.Any<JsonObject>());

        CloudflareClient.Received(1).CreateDnsRecord(
            httpEndpoint.CloudflareZoneId, Arg.Is<JsonObject>(arg => arg["type"]!.GetValue<string>() == "CNAME" &&
                                                                     arg["name"]!.GetValue<string>() ==
                                                                     httpEndpoint.ResultHost &&
                                                                     arg["content"]!.GetValue<string>() ==
                                                                     $"{tunnel.TunnelId}.cfargotunnel.com"
            )
        );

        DockerClient.Networks.Received(1).ConnectNetworkAsync(
            httpEndpoint.NetworkName,
            Arg.Is<NetworkConnectParameters>(arg => arg.Container == "cloudflare-tunnel")
        );
    }

    [Fact]
    public void Uninstall_Frierun_Success()
    {
        InstallPackage("docker");
        var tunnelApplication = InstallPackage("cloudflare-tunnel");
        var tunnel = tunnelApplication.GetContracts<CloudflareTunnel>().Single();
        Assert.NotNull(tunnel.TunnelId);
        Assert.NotNull(tunnel.AccountId);

        var application = InstallPackage("frierun");

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        Assert.NotNull(httpEndpoint.CloudflareZoneId);
        Assert.NotNull(httpEndpoint.NetworkName);

        UninstallApplication(application);

        DockerClient.Networks.Received(1).DisconnectNetworkAsync(
            httpEndpoint.NetworkName,
            Arg.Is<NetworkDisconnectParameters>(arg => arg.Container == "cloudflare-tunnel")
        );
    }

    [Fact]
    public void Install_DomainSet_FindsZone()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [httpId] = http with { ResultHost = "subdomain.domain2.zone2" } }
        };

        var application = InstallPackage(package);

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        Assert.Equal("zoneId2", httpEndpoint.CloudflareZoneId);
    }

    [Fact]
    public void Install_RootZoneAsDomain_FindsZone()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [httpId] = http with { ResultHost = "domain2.zone2" } }
        };

        var application = InstallPackage(package);

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        Assert.Equal("zoneId2", httpEndpoint.CloudflareZoneId);
    }

    [Fact]
    public void Install_NoZones_ThrowsException()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        CloudflareClient.GetZones().Returns([]);
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [httpId] = http }
        };

        var exception = Assert.Throws<HandlerException>(() => InstallPackage(package));

        Assert.Equal("No DNS zones found in Cloudflare.", exception.Message);
    }

    [Fact]
    public void Install_UnknownZone_ThrowsException()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [httpId] = http with { ResultHost = "subdomain.unknown.zone" } }
        };

        var exception = Assert.Throws<HandlerException>(() => InstallPackage(package));

        Assert.Equal(
            "No DNS zone found for subdomain.unknown.zone in Cloudflare.",
            exception.Message
        );
    }

    [Fact]
    public void Install_NoIngressInTunnelConfiguration_AddsDefaultFallback()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        CloudflareClient.GetTunnelConfiguration(Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(new JsonObject());
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [httpId] = http } };

        var application = InstallPackage(package);

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        var container = application.GetContracts<Container>().Single();
        var host = $"http://{container.ContainerName}:{httpEndpoint.Port}";
        CloudflareClient.UpdateTunnelConfiguration(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Is<JsonObject>(config =>
                config["ingress"]!.AsArray().Count == 2
                && config["ingress"]!.AsArray()[0]!["hostname"]!.GetValue<string>() == httpEndpoint.ResultHost
                && config["ingress"]!.AsArray()[0]!["service"]!.GetValue<string>() == host
                && config["ingress"]!.AsArray()[1]!["service"]!.GetValue<string>() == "http_status:404"
            )
        );
    }

    [Fact]
    public void Install_ExistingIngressInTunnelConfiguration_UpdatesIngress()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        CloudflareClient.GetTunnelConfiguration(Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(
                new JsonObject
                {
                    ["ingress"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["hostname"] = "existing.domain",
                            ["service"] = "http://existing.service:80"
                        },
                        new JsonObject
                        {
                            ["service"] = "http_status:404"
                        }
                    }
                }
            );
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [httpId] = http } };

        var application = InstallPackage(package);

        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        var container = application.GetContracts<Container>().Single();
        var host = $"http://{container.ContainerName}:{httpEndpoint.Port}";
        CloudflareClient.UpdateTunnelConfiguration(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Is<JsonObject>(config =>
                config["ingress"]!.AsArray().Count == 3
                && config["ingress"]!.AsArray()[0]!["hostname"]!.GetValue<string>() == httpEndpoint.ResultHost
                && config["ingress"]!.AsArray()[0]!["service"]!.GetValue<string>() == host
                && config["ingress"]!.AsArray()[1]!["hostname"]!.GetValue<string>() == "existing.domain"
                && config["ingress"]!.AsArray()[1]!["service"]!.GetValue<string>() == "http://existing.service:80"
                && config["ingress"]!.AsArray()[2]!["service"]!.GetValue<string>() == "http_status:404"
            )
        );
    }

    [Fact]
    public void Install_HasExistingDnsRecords_RemovesDnsRecords()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");

        CloudflareClient.GetDnsRecords("zoneId1").Returns(
            new List<JsonObject>
            {
                new() { ["id"] = "recordId1", ["name"] = "test.domain1.zone1" },
                new() { ["id"] = "recordId2", ["name"] = "test.domain1.zone1" },
                new() { ["id"] = "recordId3", ["name"] = "domain1.zone1" },
                new() { ["id"] = "recordId4", ["name"] = "other.domain" },
                new() { ["id"] = "recordId5" }
            }
        );
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with
        {
            Contracts = new ContractList { [httpId] = http with { ResultHost = "test.domain1.zone1" } }
        };

        var application = InstallPackage(package);
        
        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        Assert.Equal("zoneId1", httpEndpoint.CloudflareZoneId);

        CloudflareClient.Received(1).DeleteDnsRecord("zoneId1", "recordId1");
        CloudflareClient.Received(1).DeleteDnsRecord("zoneId1", "recordId2");
        CloudflareClient.Received(2).DeleteDnsRecord(Arg.Any<string>(), Arg.Any<string>());
    }


    [Fact]
    public void Uninstall_HasDnsRecords_RemovesDnsRecords()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        var application = InstallPackage("frierun");
        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        Assert.NotNull(httpEndpoint.CloudflareZoneId);
        CloudflareClient.GetDnsRecords(httpEndpoint.CloudflareZoneId).Returns(
            new List<JsonObject>
            {
                new() { ["id"] = "recordId1", ["name"] = httpEndpoint.ResultHost.Value },
                new() { ["id"] = "recordId2", ["name"] = httpEndpoint.ResultHost.Value },
                new() { ["id"] = "recordId3", ["name"] = "other.domain" },
                new() { ["id"] = "recordId4" }
            }
        );

        UninstallApplication(application);

        CloudflareClient.Received(1).DeleteDnsRecord(httpEndpoint.CloudflareZoneId, "recordId1");
        CloudflareClient.Received(1).DeleteDnsRecord(httpEndpoint.CloudflareZoneId, "recordId2");
        CloudflareClient.Received(2).DeleteDnsRecord(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public void Uninstall_HasIngressRules_RemovesIngressRules()
    {
        InstallPackage("docker");
        InstallPackage("cloudflare-tunnel");
        var (httpId, http) = Contract<HttpEndpoint>().GenerateEntry();
        var package = Factory<Package>().Generate() with { Contracts = new ContractList { [httpId] = http } };
        var application = InstallPackage(package );
        var httpEndpoint = application.GetContracts<HttpEndpoint>().Single();
        CloudflareClient.GetTunnelConfiguration(Arg.Any<string>(), Arg.Any<string>())
            .ReturnsForAnyArgs(
                new JsonObject
                {
                    ["ingress"] = new JsonArray
                    {
                        new JsonObject { ["hostname"] = "other.domain", ["service"] = "http://other.service:80" },
                        new JsonObject
                            { ["hostname"] = httpEndpoint.ResultHost.Value, ["service"] = "http://endpoint:80" },
                        new JsonObject { ["hostname"] = httpEndpoint.ResultHost.Value, ["service"] = "http://old:80" },
                        new JsonObject { ["service"] = "http_status:404" }
                    }
                }
            );


        UninstallApplication(application);

        CloudflareClient.Received(1).UpdateTunnelConfiguration(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Is<JsonObject>(config =>
                config["ingress"]!.AsArray().Count == 2
                && config["ingress"]!.AsArray()[0]!["hostname"]!.GetValue<string>() == "other.domain"
                && config["ingress"]!.AsArray()[0]!["service"]!.GetValue<string>() == "http://other.service:80"
                && config["ingress"]!.AsArray()[1]!["service"]!.GetValue<string>() == "http_status:404"
            )
        );
    }
}