using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Substitute = NSubstitute.Substitute;

namespace Frierun.Tests.Services;

public class InstallServiceTests : BaseTests
{
    [Fact]
    public void Handle_SuccessExecutionPlan_CallsInstall()
    {
        var application = GetFactory<Application>().Generate(); 
        var executionPlan = Substitute.For<IExecutionPlan>();
        executionPlan.Install().Returns(application);
        var service = Resolve<InstallService>();

        service.Handle(executionPlan);

        executionPlan.Received(1).Install();
    }
    
    [Fact]
    public void Handle_SuccessExecutionPlan_ChangesState()
    {
        var stateManager = Resolve<StateManager>();
        var application = GetFactory<Application>().Generate(); 
        var executionPlan = Substitute.For<IExecutionPlan>();
        executionPlan.Install().Returns(application).AndDoes(_ => Assert.False(stateManager.Ready));
        var service = Resolve<InstallService>();
        
        service.Handle(executionPlan);

        Assert.True(stateManager.Ready);
    }

    [Fact]
    public void Handle_FailedExecutionPlan_ResetsState()
    {
        var stateManager = Resolve<StateManager>();
        var executionPlan = Substitute.For<IExecutionPlan>();
        executionPlan.Install().Throws(_ => new Exception());
        var service = Resolve<InstallService>();
        
        service.Handle(executionPlan);

        Assert.True(stateManager.Ready);
    }
}