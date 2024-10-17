using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public class ExecutionPlan(State state, Package package, string name)
{
    public State State => state;
    public string Name { get; set; } = name;
    public Package Package => package;
    public IDictionary<ResourceDefinition, Provider> Providers { get; } = new Dictionary<ResourceDefinition, Provider>();
    public IDictionary<ResourceDefinition, IDictionary<string, string>> Parameters { get; } = new Dictionary<ResourceDefinition, IDictionary<string, string>>();
    public List<ContainerProvider.ExtendContainerParameters> ContainerParameters { get; } = [];

    public IEnumerable<ResourceDefinition> Resources => package.Resources;
    
    public IEnumerable<ResourceDefinition> ResourcesToInstall
    {
        get
        {
            foreach (var resourceDefinition in package.Resources)
            {
                if (resourceDefinition.ResourceType == typeof(Container))
                {
                    continue;
                }

                yield return resourceDefinition;
            }

            foreach (var resourceDefinition in package.Resources)
            {
                if (resourceDefinition.ResourceType != typeof(Container))
                {
                    continue;
                }

                yield return resourceDefinition;
            }
        }
    }
}