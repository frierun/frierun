using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class PackageRegistry
{
    public IList<Package> Packages { get; } =
    [
        // https://hub.docker.com/r/linuxserver/heimdall/
        new("Heimdall", "lscr.io/linuxserver/heimdall:latest", 80, ["/config"]),
        
        // https://github.com/bastienwirtz/homer
        new("Homer", "b4bz/homer:latest", 8080, ["/www/assets"]),
        
        // https://homarr.dev/docs/getting-started/installation/
        new("Homarr", "ghcr.io/ajnart/homarr:latest", 7575, ["/app/data/configs", "/app/public/icons", "/data"], true),
        
        // https://dashy.to/docs/quick-start
        new("Dashy", "lissy93/dashy:latest", 8080, ["/app/user-data"]),
        
        // https://docs.portainer.io/start/install-ce/server/docker/linux
        new("Portainer", "portainer/portainer-ce:latest", 8000, ["/data"], true)
    ];

    public Package? Find(string name)
    {
        return Packages.FirstOrDefault(p => p.Name == name);
    }
}