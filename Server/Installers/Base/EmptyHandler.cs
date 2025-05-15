using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class EmptyHandler : IHandler
{
    public Application? Application => null;

    public void Uninstall(Resource resource)
    {
    }
}