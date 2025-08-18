using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class ParameterHandler : Handler<Parameter>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Parameter contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Value = contract.Value.Empty ? new Argument<string>(contract.DefaultValue) : contract.Value, 
                Handler = this
            }
        );
    }
}