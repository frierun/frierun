﻿using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class ParameterHandler : Handler<Parameter>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Parameter contract, string prefix)
    {
        var value = contract.Value ?? contract.DefaultValue ?? "";

        yield return new ContractInitializeResult(
            contract with
            {
                Value = value, 
                Handler = this
            }
        );
    }
}