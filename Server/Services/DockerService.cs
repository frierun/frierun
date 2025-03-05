using System.Formats.Tar;
using System.IO.Pipelines;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Frierun.Server;

public class DockerService(ILogger<DockerService> logger, IDockerClient client)
{
    /// <summary>
    /// Starts container with specified name, image and port
    /// </summary>
    public async Task<bool> StartContainer(CreateContainerParameters dockerParameters)
    {
        logger.LogDebug("Starting container {ContainerName}", dockerParameters.Name);

        logger.LogDebug("Loading image {ImageName}", dockerParameters.Image);

        await client.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = dockerParameters.Image
            },
            null,
            new Progress<JSONMessage>()
        );

        var result = await client.Containers.CreateContainerAsync(dockerParameters);

        logger.LogDebug("Starting container {ContainerId}", result.ID);

        var started = await client.Containers.StartContainerAsync(
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
    /// Execute command in the container
    /// </summary>
    public async Task<(string stdout, string stderr)> ExecInContainer(string containerName, IList<string> command)
    {
        var execCreateParams = new ContainerExecCreateParameters
        {
            AttachStderr = true,
            AttachStdin = true,
            AttachStdout = true,
            Cmd = command,
            Tty = true
        };
        var result = await client.Exec.ExecCreateContainerAsync(containerName, execCreateParams);
        var execId = result.ID;
        if (execId == null)
        {
            throw new Exception("Exec is not created");
        }
        var stream = await client.Exec.StartAndAttachContainerExecAsync(execId, true);
        return await stream.ReadOutputToEndAsync(default);
    }

    /// <summary>
    /// Starts a fake container with specified volume, puts a file in it and stops it
    /// </summary>
    public async Task<bool> PutFile(string volumeName, string path, string content)
    {
        logger.LogDebug("Putting file {Path} to volume {VolumeName}", path, volumeName);

        var imageName = "alpine:latest";
        logger.LogDebug("Loading image {ImageName}", imageName);

        await client.Images.CreateImageAsync(
            new ImagesCreateParameters
            {
                FromImage = imageName
            },
            null,
            new Progress<JSONMessage>()
        );
        
        logger.LogDebug("Creating temporary container");
        var result = await client.Containers.CreateContainerAsync(
            new CreateContainerParameters()
            {
                Image = imageName,
                HostConfig = new HostConfig()
                {
                    Mounts = new List<Mount>
                    {
                        new()
                        {
                            Source = volumeName,
                            Target = "/mnt",
                            Type = "volume",
                        }
                    }
                },
            }
        );

        var containerId = result.ID;

        logger.LogDebug("Starting container {ContainerId}", containerId);

        var started = await client.Containers.StartContainerAsync(
            containerId,
            new ContainerStartParameters()
        );

        if (!started)
        {
            logger.LogError("Failed to start container");
            return false;
        }

        var pipe = new Pipe();
        await using (var tarArchive = new TarWriter(pipe.Writer.AsStream()))
        {
            await tarArchive.WriteEntryAsync(
                new PaxTarEntry(TarEntryType.RegularFile, path)
                {
                    DataStream = new MemoryStream(Encoding.UTF8.GetBytes(content))
                }
            );
        }

        logger.LogDebug("Putting file {path} to container {ContainerId}", path, containerId);
        await client.Containers.ExtractArchiveToContainerAsync(
            result.ID,
            new ContainerPathStatParameters
            {
                Path = "/mnt"
            },
            pipe.Reader.AsStream()
        );

        logger.LogDebug("Deleting temporary container {ContainerId}", containerId);

        await client.Containers.RemoveContainerAsync(
            containerId,
            new ContainerRemoveParameters { RemoveVolumes = false }
        );

        return true;
    }

    /// <summary>
    /// Stops and deletes container with specified name
    /// </summary>
    public async Task<bool> StopContainer(string containerName)
    {
        logger.LogDebug("Stopping container {ContainerName}", containerName);

        var response = await client.Containers.ListContainersAsync(
            new ContainersListParameters
            {
                All = true
            }
        );

        var container = response.FirstOrDefault(c => c.Names.Contains($"/{containerName}"));
        if (container == null)
        {
            logger.LogError("Container {ContainerName} not found", containerName);
            return true;
        }

        if (container.State == "running")
        {
            logger.LogDebug("Stopping container {ContainerName}", containerName);
            if (!await client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters()))
            {
                logger.LogError("Failed to stop container {ContainerName}", containerName);
                return false;
            }
        }

        await client.Containers.RemoveContainerAsync(
            container.ID,
            new ContainerRemoveParameters { RemoveVolumes = true }
        );

        return true;
    }

    /// <summary>
    /// Removes volume by name
    /// </summary>
    public async Task<bool> CreateVolume(string volumeName)
    {
        try
        {
            await client.Volumes.CreateAsync(
                new VolumesCreateParameters()
                {
                    Name = volumeName
                }
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove volume {VolumeName}", volumeName);
            return false;
        }

        return true;
    }


    /// <summary>
    /// Removes volume by name
    /// </summary>
    public async Task<bool> RemoveVolume(string volumeName)
    {
        try
        {
            await client.Volumes.RemoveAsync(volumeName, true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove volume {VolumeName}", volumeName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates new network for a container group
    /// </summary>
    public async Task<bool> CreateNetwork(string networkName)
    {
        logger.LogDebug("Creating network {NetworkName}", networkName);
        try
        {
            await client.Networks.CreateNetworkAsync(
                new NetworksCreateParameters
                {
                    Name = networkName
                }
            );
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create network {NetworkName}", networkName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes network
    /// </summary>
    public async Task<bool> RemoveNetwork(string networkName)
    {
        logger.LogDebug("Removing network {NetworkName}", networkName);
        try
        {
            await client.Networks.DeleteNetworkAsync(networkName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove network {NetworkName}", networkName);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Attaches container to network
    /// </summary>
    public async Task<bool> AttachNetwork(string networkName, string containerName)
    {
        logger.LogDebug(
            "Attaching container {ContainerName} to network {NetworkName}", containerName, networkName
        );
        try
        {
            await client.Networks.ConnectNetworkAsync(
                networkName, new NetworkConnectParameters
                {
                    Container = containerName
                }
            );
        }
        catch (Exception e)
        {
            logger.LogError(
                e, "Failed to attach container {ContainerName} to network {NetworkName}", containerName, networkName
            );
            return false;
        }

        return true;
    }

    /// <summary>
    /// Detaches container from network
    /// </summary>
    public async Task<bool> DetachNetwork(string networkName, string containerName)
    {
        logger.LogDebug(
            "Detaching container {ContainerName} to network {NetworkName}", containerName, networkName
        );
        try
        {
            await client.Networks.DisconnectNetworkAsync(
                networkName, new NetworkDisconnectParameters()
                {
                    Container = containerName
                }
            );
        }
        catch (Exception e)
        {
            logger.LogError(
                e, "Failed to detach container {ContainerName} from network {NetworkName}", containerName, networkName
            );
            return false;
        }

        return true;
    }
}