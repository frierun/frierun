using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PasswordInstaller : IInstaller<Password>
{
    public Application? Application => null;

    Password IInstaller<Password>.Install(Password contract, ExecutionPlan plan)
    {
        var password = RandomNumberGenerator.GetString(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
            16
        );

        return contract with
        {
            Result = new GeneratedPassword(new EmptyHandler()) { Value = password }
        };
    }
}