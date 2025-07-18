﻿using Frierun.Server.Data;

namespace Frierun.Tests.Handlers.Base;

public class PackageHandlerTests : BaseTests
{
    public PackageHandlerTests()
    {
        InstallPackage("docker");
    }

    [Fact]
    public void ApplicationUrl_Complete_ApplicationUrlHasPriority()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts = new List<Contract>
            {
                new HttpEndpoint(),
                new PortEndpoint(
                    Protocol.Tcp,
                    2222
                )
            }
        };
        Assert.NotNull(package.ApplicationUrl);

        var application = InstallPackage(package);

        Assert.Equal(package.ApplicationUrl, application.Url);
    }
    
    [Fact]
    public void ApplicationUrl_WithHttpAndPortEndpoint_HttpEndpointHasPriority()
    {
        var package = Factory<Package>().Generate() with
        {
            ApplicationUrl = null,
            Contracts = new List<Contract>
            {
                new HttpEndpoint(),
                new PortEndpoint(
                    Protocol.Tcp,
                    2222
                )
            }
        };

        var application = InstallPackage(package);

        Assert.Equal("http://127.0.0.1/", application.Url);
    }
    
    [Fact]
    public void ApplicationUrl_WithPortEndpoint_AutoDetectPortEndpoint()
    {
        var package = Factory<Package>().Generate() with
        {
            ApplicationUrl = null,
            Contracts = new List<Contract>
            {
                new PortEndpoint(
                    Protocol.Tcp,
                    2222
                )
            }
        };

        var application = InstallPackage(package);

        Assert.Equal("tcp://127.0.0.1:2222", application.Url);
    }    
}