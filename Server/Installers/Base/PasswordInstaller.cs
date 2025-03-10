﻿using System.Security.Cryptography;
using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class PasswordInstaller : IInstaller<Password>, IUninstaller<GeneratedPassword>
{
    /// <inheritdoc />
    Resource IInstaller<Password>.Install(Password contract, ExecutionPlan plan)
    {
        var password = RandomNumberGenerator.GetString(
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
            16
        );
        return new GeneratedPassword(password);
    }
}