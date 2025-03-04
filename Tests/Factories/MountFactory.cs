﻿using Bogus;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Tests.Factories;

public sealed class MountFactory: Faker<Mount>
{
    public MountFactory()
    {
        StrictMode(true);
        CustomInstantiator(_ => new Mount("", ""));
        RuleFor(p => p.Path, f => f.System.DirectoryPath());
        RuleFor(p => p.VolumeName, f => f.Lorem.Word());
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        RuleFor(p => p.ReadOnly, f => f.Random.Bool());
        Ignore(p => p.Installer);
        Ignore(p => p.Name);
    }
}