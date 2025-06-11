using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class StaticDomainHandler(Application application)
    : Handler<Domain>(application)
{
    private readonly string _domainName = application.Contracts
        .OfType<Parameter>()
        .First(parameter => parameter.Name == "Domain")
        .Value ?? "";

    private readonly bool _isInternal = application.Contracts
        .OfType<Selector>()
        .First(parameter => parameter.Name == "Internal")
        .Value == "Yes";

    public override IEnumerable<ContractInitializeResult> Initialize(Domain contract, string prefix)
    {
        if (contract.IsInternal != null && contract.IsInternal != _isInternal)
        {
            // Can't fulfil the request
            yield break;
        }

        if (contract.Value != null)
        {
            if (contract.Value != _domainName && !contract.Value.EndsWith($".{_domainName}"))
            {
                // Can't fulfil the request
                yield break;
            }

            if (!IsDomainExist(contract.Value))
            {
                yield return new ContractInitializeResult(contract with { Handler = this, IsInternal = _isInternal });
            }

            yield break;
        }
        
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this,
                Value = FindUniqueName(
                    prefix,
                    c => c.Value,
                    $".{_domainName}"
                ),
                IsInternal = _isInternal
            }
        );
    }

    /// <summary>
    /// Checks if subdomain is already in use
    /// </summary>
    private bool IsDomainExist(string domain)
    {
        return State.Contracts.OfType<Domain>().Any(c => c.Value == domain);
    }
}