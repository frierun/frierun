using Docker.DotNet.Models;
using Frierun.Server.Models;

namespace Frierun.Server.Providers;

public interface IChangesContainer
{
    public void ChangeContainer(ExecutionPlan plan, CreateContainerParameters parameters);
}