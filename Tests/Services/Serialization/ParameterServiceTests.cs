using Frierun.Server.Models;
using Frierun.Server.Services;

namespace Frierun.Tests.Services;

public class ParameterServiceTests : BaseTests
{
    [Fact]
    public void GetDefaultName_WhenNoApplications_ReturnsPackageName()
    {
        var package = GetFactory<Package>().Generate();
        var service = GetService<ParameterService>();

        var result = service.GetDefaultName(package);

        Assert.Equal(package.Name, result);
    }

    [Fact]
    public void GetDefaultName_WhenApplicationWithSameNameExists_ReturnsNameWithCount()
    {
        var package = GetFactory<Package>().Generate();
        var application = GetFactory<Application>().RuleFor(p => p.Name, package.Name).Generate();
        GetState().Applications.Add(application);
        var service = GetService<ParameterService>();

        var result = service.GetDefaultName(package);

        Assert.Equal($"{package.Name}-1", result);
    }
    
    [Fact]
    public void GetDefaultName_WhenVolumeWithSameNameExists_ReturnsNameWithCount()
    {
        var volume = GetFactory<Volume>().Generate();
        var package = GetFactory<Package>().RuleFor(p => p.Volumes, () => new List<Volume> {volume}).Generate();
        var application = GetFactory<Application>().RuleFor(p => p.VolumeNames, new List<string> { $"{package.Name}-{volume.Name}" }).Generate();
        GetState().Applications.Add(application);
        var service = GetService<ParameterService>();

        var result = service.GetDefaultName(package);

        Assert.Equal($"{package.Name}-1", result);
    }

    [Fact]
    public void GetDefaultPort_WhenNoApplications_Returns80()
    {
        var service = GetService<ParameterService>();

        var result = service.GetDefaultPort();

        Assert.Equal(80, result);
    }

    [Fact]
    public void GetDefaultPort_WhenApplicationWithSamePortExists_ReturnsNextPort()
    {
        var application = GetFactory<Application>().RuleFor(p => p.Port, 80).Generate();
        GetState().Applications.Add(application);
        var service = GetService<ParameterService>();
        
        var result = service.GetDefaultPort();

        Assert.Equal(8080, result);
    }
}