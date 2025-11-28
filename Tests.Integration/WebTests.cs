using Autofac;
using Frierun.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Integration;

public class WebTests : BaseTests, IDisposable
{
    private readonly IHost _webHost;
    private readonly HttpClient _client;
    
    public WebTests()
    {
        var serve = new Serve(Resolve<ILifetimeScope>());
        _webHost = serve.CreateWebApplication(builder => builder.WebHost.UseTestServer());
        
        _webHost.Start();
        _client =  _webHost.GetTestClient();
    }

    public new void Dispose()
    {
        _client.Dispose();
        _webHost.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData("/api/v1/applications")]
    [InlineData("/api/v1/packages")]
    public async Task Get_Endpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var response = await _client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}