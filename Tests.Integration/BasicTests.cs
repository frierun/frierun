using Frierun.Server;
using Microsoft.Extensions.DependencyInjection;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Tests.Integration;

public class BasicTests : BaseTests
{
    [Theory]
    [InlineData("/api/v1/applications")]
    [InlineData("/api/v1/packages")]
    public async Task Get_Endpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var client = CreateClient();

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public void Finding_Frierun_ShouldReturnPackage()
    {
        var package = Services.GetRequiredService<PackageRegistry>().Find("frierun");

        Assert.NotNull(package);
    }
}