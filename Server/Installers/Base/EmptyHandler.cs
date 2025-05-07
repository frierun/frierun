using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class EmptyHandler : IHandler
{
    public static Lazy<IHandler> CreateLazy() 
    {
        return new Lazy<IHandler>(() => new EmptyHandler());
    }
    
    /// <inheritdoc />
    public Application? Application => null;

    /// <inheritdoc />
    public void Uninstall(Resource resource)
    {
    }
}