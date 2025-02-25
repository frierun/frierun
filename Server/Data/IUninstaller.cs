using System.Diagnostics;

namespace Frierun.Server.Data;

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