using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public class StaticDomainInstaller(State state, Application application)
    : IInstaller<Domain>, IUninstaller<ResolvedDomain>
{
    private readonly string _domainName = application.Resources.OfType<ResolvedParameter>().First().Value ?? "";

    /// <inheritdoc />
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
        return state.Resources.OfType<ResolvedDomain>().Any(c => c.Value == fullDomain);
    }

    /// <inheritdoc />
    Resource? IInstaller<Domain>.Install(Domain contract, ExecutionPlan plan)
    {
        return new ResolvedDomain(
            string.IsNullOrEmpty(contract.Subdomain)
                ? _domainName
                : $"{contract.Subdomain}.{_domainName}"
        );
    }
}