using Docker.DotNet;
using Docker.DotNet.Models;
using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class DockerService(ILogger<DockerService> logger)
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    /// <summary>
    /// Starts container with specified name, image and port
    /// </summary>
    public async Task<bool> StartContainer(string containerName, int externalPort, Package package)
    {
        logger.LogInformation("Starting container {ContainerName} from package {PackageName}", containerName, package.Name);

        await _client.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = package.ImageName
            },
            null,
            new Progress<JSONMessage>()
        );

        var mounts = new List<Mount>();
        if (package.Volumes != null)
        {
            mounts.AddRange(package.Volumes.Select(volume => new Mount
            {
                Source = $"{containerName}-{volume.Name}",
                Target = volume.Path,
                Type = "volume"
            }));
        }

        var result = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = package.ImageName,
            Name = containerName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        $"{package.Port}/tcp",
                        new List<PortBinding>
                        {
                            new()
                            {
                                HostPort = externalPort.ToString()
                            }
                        }
                    }
                },
                Mounts = mounts,
            },
        });

        logger.LogInformation("Started container {ContainerId}", result.ID);

        var started = await _client.Containers.StartContainerAsync(
            result.ID,
            new ContainerStartParameters()
        );

        if (!started)
        {
            logger.LogError("Failed to start container");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Stops and deletes container with specified name
    /// </summary>
    public async Task<bool> StopContainer(string containerName)
    {
        logger.LogInformation("Stopping container {ContainerName}", containerName);

        var container = await _client.Containers.ListContainersAsync(new ContainersListParameters
        {
            All = true
        });

        var containerId = container.FirstOrDefault(c => c.Names.Contains($"/{containerName}"))?.ID;

        if (containerId == null)
        {
            logger.LogError("Container {ContainerName} not found", containerName);
            return true;
        }

        if (!await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters()))
        {
            logger.LogError("Failed to stop container {ContainerName}", containerName);
            return false;
        }

        await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { RemoveVolumes = true });

        return true;
    }

    /// <summary>
    /// Removes volume by name
    /// </summary>
    public async Task RemoveVolume(string volumeName)
    {
        await _client.Volumes.RemoveAsync(volumeName, true);
    }
}