using Frierun.Server.Models;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class ParameterServiceTests : BaseTests
{
    [Fact]
    public void GetDefaultName_WhenNoApplications_ReturnsPackageName()
    {
        var package = GetFactory<Package>().Generate();
        var state = new State();
        var service = new ParameterService(state);
        
        var result = service.GetDefaultName(package);
        
        Assert.Equal(package.Name, result);
    }
    
    [Fact]
    public void GetDefaultName_WhenApplicationWithSameNameExists_ReturnsNameWithCount()
    {
        var package = GetFactory<Package>().Generate();
        var state = new State();
        state.Applications.Add(new Application(Guid.NewGuid(), package.Name, 80, package));
        var service = new ParameterService(state);
        
        var result = service.GetDefaultName(package);
        
        Assert.Equal($"{package.Name}-1", result);
    }
    
    [Fact]
    public void GetDefaultPort_WhenNoApplications_Returns80()
    {
        var state = new State();
        var service = new ParameterService(state);
        
        var result = service.GetDefaultPort();
        
        Assert.Equal(80, result);
    }
    
    [Fact]
    public void GetDefaultPort_WhenApplicationWithSamePortExists_ReturnsNextPort()
    {
        var state = new State();
        state.Applications.Add(new Application(Guid.NewGuid(), "app", 80, GetFactory<Package>().Generate()));
        var service = new ParameterService(state);
        
        var result = service.GetDefaultPort();
        
        Assert.Equal(8080, result);
    }
}