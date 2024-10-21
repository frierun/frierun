using Docker.DotNet.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public class ContainerExecutionPlan(
    State state,
    ContainerDefinition definition,
    ContainerProvider provider,
    ExecutionPlan? parent
) : ExecutionPlan<ContainerDefinition>(state, definition, provider, parent)
{
    public delegate void StartContainerDelegate(CreateContainerParameters parameters);
    public event StartContainerDelegate? StartContainer;

    public void OnStartContainer(CreateContainerParameters parameters)
    {
        StartContainer?.Invoke(parameters);
    }
}