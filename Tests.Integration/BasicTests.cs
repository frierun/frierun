using Frierun.Server.Services;
using Microsoft.Extensions.DependencyInjection;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Tests.Integration;

using Microsoft.AspNetCore.Mvc.Testing;

public class BasicTests
{
    private readonly WebApplicationFactory<Program> _factory = new();

    [Theory]
    [InlineData("/api/v1/applications")]
    [InlineData("/api/v1/packages")]
    public async Task Get_Endpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public void Finding_Frierun_ShouldReturnPackage()
    {
        var package = _factory.Services.GetRequiredService<PackageRegistry>().Find("frierun");

        Assert.NotNull(package);
    }
}