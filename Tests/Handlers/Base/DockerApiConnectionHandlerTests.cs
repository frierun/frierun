using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class DockerApiConnectionHandlerTests : BaseTests
{
    [Fact]
    public void Install_WrongPath_ThrowsHandlerException()
    {
        var package = Factory<Package>().Generate() with
        {
            Contracts =
            [
                new DockerApiConnection
                {
                    Path = "wrong_path",
                    Handler = Handler<DockerApiConnectionHandler>()
                }
            ]
        };

        var exception = Assert.Throws<HandlerException>(() => InstallPackage(package));
        Assert.Equal("Docker API connection failed.", exception.Message);
    }
}