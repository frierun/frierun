using Frierun.Server.Data;
using Frierun.Server.Handlers;
using Frierun.Server.Handlers.Base;

namespace Frierun.Tests.Handlers.Base;

public class DockerApiConnectionHandlerTests : BaseTests
{
    [Fact]
    public void Install_WrongPath_ThrowsHandlerException()
    {
        var handler = new DockerApiConnectionHandler();
        var contract = new DockerApiConnection
        {
            Path = "wrong_path"
        };
        var plan = new ExecutionPlan(new Dictionary<ContractId, Contract>());

        Assert.Throws<HandlerException>(() => handler.Install(contract, plan));
    }
}