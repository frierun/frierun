using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public interface IContainerHandler : IHandler<DockerContainer>
{
    void AttachNetwork(DockerContainer container, string networkName);
    void DetachNetwork(DockerContainer container, string networkName);
    Task<(string stdout, string stderr)> ExecInContainer(DockerContainer container, IList<string> command);
}