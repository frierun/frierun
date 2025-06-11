using Bogus;
using Frierun.Server;
using Frierun.Server.Data;
using Frierun.Server.Handlers;

namespace Frierun.Tests;

public class StateManagerTests : BaseTests
{
    [Fact]
    public void StartTask_InitialState_SetsReadyToFalse()
    {
        var taskName = Resolve<Faker>().Lorem.Word();
        var stateManager = Resolve<StateManager>();
        Assert.True(stateManager.Ready);

        var result = stateManager.StartTask(taskName);
        Assert.True(result);
        Assert.False(stateManager.Ready);
        Assert.Equal(taskName, stateManager.TaskName);
        Assert.Null(stateManager.Exception);
    }

    [Fact]
    public void StartTask_WhenAlreadyRunning_ReturnsFalse()
    {
        var taskName = Resolve<Faker>().Lorem.Word();
        var stateManager = Resolve<StateManager>();
        Assert.True(stateManager.Ready);

        var result = stateManager.StartTask(taskName);
        Assert.True(result);
        Assert.False(stateManager.Ready);

        // Try to start another task
        var anotherTaskName = Resolve<Faker>().Lorem.Word();
        var anotherResult = stateManager.StartTask(anotherTaskName);
        Assert.False(anotherResult);
        Assert.Equal(taskName, stateManager.TaskName);
    }

    [Fact]
    public void FinishTask_WhenRunning_SetsReadyToTrue()
    {
        var taskName = Resolve<Faker>().Lorem.Word();
        var stateManager = Resolve<StateManager>();
        Assert.True(stateManager.Ready);

        stateManager.StartTask(taskName);
        Assert.False(stateManager.Ready);

        stateManager.FinishTask();
        Assert.True(stateManager.Ready);
        Assert.Equal(string.Empty, stateManager.TaskName);
    }

    [Fact]
    public void StartTask_WhenExceptionIsSet_ResetsException()
    {
        var taskName = Resolve<Faker>().Lorem.Word();
        var stateManager = Resolve<StateManager>();
        Assert.True(stateManager.Ready);

        stateManager.Exception = new HandlerException("Test message", "Test solution", Factory<Container>().Generate());
        Assert.NotNull(stateManager.Exception);

        var result = stateManager.StartTask(taskName);
        Assert.True(result);
        Assert.Null(stateManager.Exception);
    }
}