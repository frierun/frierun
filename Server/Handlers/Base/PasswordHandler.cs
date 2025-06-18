using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PasswordHandler : Handler<Password>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Password contract, string prefix)
    {
        if (contract.Value != null)
        {
            yield return new ContractInitializeResult(
                contract with { Handler = this }
            );
        }
        else
        {
            yield return new ContractInitializeResult(contract with
            {
                Handler = this,
                Value = RandomNumberGenerator.GetString(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                    16
                )
            });
        }
    }
}