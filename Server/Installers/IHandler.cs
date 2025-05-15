using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public interface IHandler<in TResource>: IHandler
    where TResource : Resource
{
    void Uninstall(TResource resource);

    [DebuggerStepThrough]
    void IHandler.Uninstall(Resource resource)
    {
        Uninstall((TResource)resource);
    }
}

public interface IHandler
{
    public Application? Application { get; }
    
    void Uninstall(Resource resource);
}