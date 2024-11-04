using Frierun.Server.Models;
using Frierun.Server.Resources;
using Frierun.Server.Services;
using File = Frierun.Server.Resources.File;

namespace Frierun.Server.Providers;

public class FileProvider(DockerService dockerService) : Provider<File, FileDefinition>
{
    /// <inheritdoc />
    protected override void FillParameters(ExecutionPlan<File, FileDefinition> plan)
    {
        return;
    }

    /// <inheritdoc />
    protected override bool Validate(ExecutionPlan<File, FileDefinition> plan)
    {
        return true;
    }

    /// <inheritdoc />
    protected override File Install(ExecutionPlan<File, FileDefinition> plan)
    {
        if (plan.Parent is not ExecutionPlan<Volume, VolumeDefinition> volumePlan)
        {
            throw new Exception("Parent plan must be a volume");
        }
        
        var volumeName = volumePlan.Parameters["name"];
        
        dockerService.PutFile(volumeName, plan.Definition.Path, plan.Definition.Text).Wait();

        return new File();
    }

    /// <inheritdoc />
    protected override void Uninstall(File resource)
    {
    }
}