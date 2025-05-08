using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests;

public class UninstallServiceTests : BaseTests
{
    [Fact]
    public void Handle_FrierunApplication_ClearsState()
    {
        var docker = InstallPackage("docker");
        var frierun = InstallPackage("frierun");
        var state = Resolve<State>();
        Assert.Equal(2, state.Applications.Count());
        var uninstallService = Resolve<UninstallService>();
        
        uninstallService.Handle(frierun);
        uninstallService.Handle(docker);

        Assert.Empty(state.Applications);
    }
    
    [Fact]
    public void Handle_DependentApplication_WorksProperlyOnCorrectOrder()
    {
        var docker = InstallPackage("docker");
        var traefik = InstallPackage("traefik");
        var frierun = InstallPackage("frierun");
        var state = Resolve<State>();
        Assert.Equal(3, state.Applications.Count());
        var uninstallService = Resolve<UninstallService>();
        
        uninstallService.Handle(frierun);
        uninstallService.Handle(traefik);
        uninstallService.Handle(docker);
        
        Assert.Empty(state.Applications);
    }    
    
    [Fact]
    public void Handle_DependentApplication_ThrowsExceptionOnWrongOrder()
    {
        InstallPackage("docker");
        InstallPackage("static-domain");
        var traefik = InstallPackage("traefik");
        var application = InstallPackage("frierun");
        
        Assert.Single(application.Resources.OfType<GenericHttpEndpoint>());
        
        Assert.Throws<Exception>(() => Resolve<UninstallService>().Handle(traefik));
    }
}