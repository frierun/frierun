using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public interface IUninstaller<in TResource>: IUninstaller
    where TResource : Resource
{
    void Uninstall(TResource resource)
    {
        
    }

    [DebuggerStepThrough]
    void IUninstaller.Uninstall(Resource resource)
    {
        Uninstall((TResource)resource);
    }
}

public interface IUninstaller
{
    void Uninstall(Resource resource);
}