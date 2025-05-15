using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public interface IContainerHandler : IHandler<Container>
{
    void AttachNetwork(Container container, string networkName);
    void DetachNetwork(Container container, string networkName);
    Task<(string stdout, string stderr)> ExecInContainer(Container container, IList<string> command);
}