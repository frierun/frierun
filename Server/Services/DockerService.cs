using Docker.DotNet;
using Docker.DotNet.Models;

namespace Frierun.Server.Services;

public class DockerService(ILogger<DockerService> logger)
{
    private readonly DockerClient _client = new DockerClientConfiguration().CreateClient();

    /// <summary>
    /// Starts container with specified name, image and port
    /// </summary>
    public async Task<bool> StartContainer(string containerName, string imageName, int port)
    {
        logger.LogInformation("Starting container {ContainerName} from image {ImageName}", containerName, imageName);

        await _client.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = imageName
            },
            null,
            new Progress<JSONMessage>()
        );

        var result = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = imageName,
            Name = containerName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        $"{port}/tcp",
                        new List<PortBinding>
                        {
                            new()
                            {
                                HostPort = "80"
                            }
                        }
                    }
                }
            }
        });

        logger.LogInformation(result.ID);

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
    /// <param name="containerName"></param>
    /// <returns></returns>
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
            return false;
        }

        if (!await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters()))
        {
            logger.LogError("Failed to stop container {ContainerName}", containerName);
            return false;
        }

        await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters() { RemoveVolumes = true });

        return true;
    }
}