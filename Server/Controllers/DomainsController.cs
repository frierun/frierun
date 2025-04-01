using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/domains")]
public class DomainsController : ControllerBase
{
    public record DomainInstaller(
        string TypeName,
        string? ApplicationName,
        string? DomainName
    );

    [HttpGet]
    public IEnumerable<DomainInstaller> List(InstallerRegistry registry)
    {
        return registry.GetInstallers(typeof(Domain)).Select(installer => new DomainInstaller(
            installer.GetType().Name,
            installer.Application?.Name,
            installer.Application?.Resources.OfType<ResolvedParameter>().FirstOrDefault()?.Value
        ));
    }
}