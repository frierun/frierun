using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers;

public class StaticDomainInstaller(State state, Application application)
    : IInstaller<Domain>
{
    private readonly string _domainName = application.Contracts
        .OfType<Parameter>()
        .First(parameter => parameter.Name == "Domain")
        .Result
        ?.Value ?? "";

    private readonly bool _isInternal = application.Contracts
        .OfType<Selector>()
        .First(parameter => parameter.Name == "Internal")
        .Result
        ?.Value == "Yes";

    public Application Application => application;

    IEnumerable<InstallerInitializeResult> IInstaller<Domain>.Initialize(Domain contract, string prefix)
    {
        if (contract.Subdomain != null && !IsSubdomainExist(contract.Subdomain))
        {
            yield return new InstallerInitializeResult(contract);
            yield break;
        }

        var count = 0;
        var subdomain = prefix;
        if (contract.Name != "")
        {
            prefix = $"{prefix}-{contract.Name}";
        }

        while (IsSubdomainExist(subdomain))
        {
            count++;
            subdomain = $"{prefix}{(count == 1 ? "" : count.ToString())}";
        }

        yield return new InstallerInitializeResult(
            contract with { Subdomain = subdomain }
        );
    }

    /// <summary>
    /// Checks if subdomain is already in use
    /// </summary>
    private bool IsSubdomainExist(string subdomain)
    {
        var fullDomain = subdomain == "" ? _domainName : $"{subdomain}.{_domainName}";
        return state.Contracts.OfType<Domain>().Any(c => c.Result?.Value == fullDomain);
    }

    Domain IInstaller<Domain>.Install(Domain contract, ExecutionPlan plan)
    {
        var domain = string.IsNullOrEmpty(contract.Subdomain)
            ? _domainName
            : $"{contract.Subdomain}.{_domainName}";
        
        return contract with
        {
            Result = new ResolvedDomain(new EmptyHandler()) { Value = domain, IsInternal = _isInternal }
        };
    }
}