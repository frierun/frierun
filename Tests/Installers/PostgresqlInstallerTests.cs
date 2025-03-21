﻿using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests.Installers;

public class PostgresqlInstallerTests : BaseTests
{
    [Fact]
    public void Install_PackageWithContract_CreatesDatabase()
    {
        var providerApplication = InstallPackage("postgresql") 
                                  ?? throw new Exception("Postgresql application not installed");
        
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Postgresql(),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        var database = application.DependsOn.OfType<PostgresqlDatabase>().First();
        Assert.Equal(package.Name, database.User);
        Assert.Equal(package.Name, database.Database);
        Assert.Single(application.DependsOn.OfType<DockerAttachedNetwork>());
        Assert.Equal([providerApplication], database.DependsOn.OfType<Application>());
    }    
    
    [Fact]
    public void Install_PackageWithTwoContracts_OnlyOneNetworkAttached()
    {
        InstallPackage("postgresql");
        
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new Postgresql("first"),
                new Postgresql("second"),
            ]
        };

        var application = InstallPackage(package);

        Assert.NotNull(application);
        Assert.Equal(2, application.DependsOn.OfType<PostgresqlDatabase>().Count());
        Assert.Single(application.DependsOn.OfType<DockerAttachedNetwork>());
    }    
}