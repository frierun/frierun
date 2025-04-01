using Frierun.Server;
using Frierun.Server.Data;

namespace Frierun.Tests.Services;

public class UninstallServiceTests : BaseTests
{
    [Fact]
    public void Handle_FrierunApplication_ClearsState()
    {
        var application = InstallPackage("frierun");
        var state = Resolve<State>();
        Assert.NotNull(application);
        Assert.Single(state.Applications);
        var uninstallService = Resolve<UninstallService>();
        
        uninstallService.Handle(application);

        Assert.Empty(state.Applications);
    }
    
    [Fact]
    public void Handle_DependentApplication_WorksProperlyOnCorrectOrder()
    {
        var traefik = InstallPackage("traefik");
        var frierun = InstallPackage("frierun");
        var state = Resolve<State>();
        Assert.Equal(2, state.Applications.Count());
        Assert.NotNull(traefik);
        Assert.NotNull(frierun);
        var uninstallService = Resolve<UninstallService>();
        
        uninstallService.Handle(frierun);
        uninstallService.Handle(traefik);
        
        Assert.Empty(state.Applications);
    }    
    
    [Fact]
    public void Handle_DependentApplication_ThrowsExceptionOnWrongOrder()
    {
        InstallPackage("static-domain");
        var traefik = InstallPackage("traefik");
        var application = InstallPackage("frierun");
        
        Assert.NotNull(traefik);
        Assert.NotNull(application);
        Assert.Single(application.Resources.OfType<TraefikHttpEndpoint>());
        
        Assert.Throws<Exception>(() => Resolve<UninstallService>().Handle(traefik));
    }
}