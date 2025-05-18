using Frierun.Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace Frierun.Server.Controllers;

[Route("/domains")]
public class DomainsController : ControllerBase
{
    public record DomainVariant(
        string TypeName,
        string? ApplicationName,
        string? DomainName
    );

    [HttpGet]
    public IEnumerable<DomainVariant> List(HandlerRegistry registry)
    {
        return registry.GetHandlers(typeof(Domain))
            .Select(
                handler => new DomainVariant(
                    handler.GetType().Name,
                    handler.Application?.Name,
                    handler.Application?.Contracts.OfType<Parameter>().FirstOrDefault()?.Result?.Value
                )
            );
    }
}