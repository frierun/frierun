using Frierun.Server;
using Microsoft.Extensions.DependencyInjection;


namespace Tests.Integration;

public class BasicTests : BaseTests
{
    [Fact]
    public void Finding_Frierun_ShouldReturnPackage()
    {
        var package = Resolve<PackageRegistry>().Find("frierun");

        Assert.NotNull(package);
    }
}