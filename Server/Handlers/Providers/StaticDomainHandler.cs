using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class StaticDomainHandler(Application application)
    : Handler<Domain>(application)
{
    private readonly string _domainName = application
        .GetContract(new ContractId<Parameter>("Domain"))
        .Value
        .Value ?? "";

    private readonly bool _isInternal = application
        .GetContract(new ContractId<Selector>("Internal"))
        .Value == "Yes";

    public override IEnumerable<ContractList> Initialize(Domain contract, ApplicationContext context)
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
                yield return [contract with { Handler = this, IsInternal = _isInternal }];
            }

            yield break;
        }

        yield return
        [
            contract with
            {
                Handler = this,
                Value = FindUniqueName(
                    context.Prefix,
                    c => c.Value,
                    $".{_domainName}"
                ),
                IsInternal = _isInternal
            }
        ];
    }

    /// <summary>
    /// Checks if a subdomain is already in use
    /// </summary>
    private bool IsDomainExist(string domain)
    {
        return State.Contracts.OfType<Domain>().Any(c => c.Value == domain);
    }
}