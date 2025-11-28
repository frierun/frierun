using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class ParameterHandler(State state) : Handler<Parameter>(state)
{
    public override IEnumerable<ContractList> Initialize(Parameter contract, ApplicationContext context)
    {
        yield return new ContractList
        {
            [context] = contract with
            {
                Value = contract.Value.Empty ? new Argument<string>(contract.DefaultValue) : contract.Value,
                Handler = this
            }
        };
    }
}