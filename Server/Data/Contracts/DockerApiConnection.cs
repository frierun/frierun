using System.Diagnostics;
using Docker.DotNet;
using Frierun.Server.Handlers;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record DockerApiConnection(
    string? Name = null,
    string? Path = null,
    bool? IsPodman = null
) : Contract<IDockerApiConnectionHandler>(Name ?? ""), IHasStrings
{
    public Contract ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Path = Path != null ? decorator(Path ?? "") : null
        };
    }

    /// <summary>
    /// Creates a Docker client using the current configuration.
    /// </summary>
    public IDockerClient CreateClient()
    {
        Debug.Assert(Handler != null);
        return Handler.CreateClient(this);
    }

    /// <summary>
    /// Gets the socket path in the root system, to be mounted in a container to control this docker instance.
    /// </summary>
    public string GetSocketRootPath()
    {
        Debug.Assert(Handler != null);
        return Handler.GetSocketRootPath(this);
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Path = OnlyOne(Path, contract.Path),
            IsPodman = OnlyOne(IsPodman, contract.IsPodman)
        };
    }
}