using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class PasswordHandler(State state) : Handler<Password>(state)
{
    public override IEnumerable<ContractList> Initialize(Password contract, ApplicationContext context)
    {
        yield return new ContractList
        {
            [context] = contract with
            {
                Handler = this,
                Value = contract.Value ?? RandomNumberGenerator.GetString(
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
                    16
                )
            }
        };
    }
}