using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public interface IContainerHandler : IHandler
{
    /// <summary>
    /// Attaches the container to a network.
    /// </summary>
    void AttachNetwork(Container container, Network network);
    
    /// <summary>
    /// Detaches the container from a network.
    /// </summary>
    void DetachNetwork(Container container, Network network);
    
    /// <summary>
    /// Executes a command in the container.
    /// </summary>
    (string stdout, string stderr) ExecInContainer(Container container, IList<string> command);
}