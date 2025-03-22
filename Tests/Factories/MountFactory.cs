using Bogus;
using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Tests.Factories;

public sealed class MountFactory: Faker<Mount>
{
    public MountFactory(Faker<Volume> volumeFactory)
    {
        StrictMode(true);
        CustomInstantiator(_ => new Mount("", ""));
        RuleFor(p => p.Path, f => f.System.DirectoryPath());
        RuleFor(p => p.VolumeName, _ => volumeFactory.Generate().Name);
        RuleFor(p => p.ContainerName, f => f.Lorem.Word());
        RuleFor(p => p.ReadOnly, f => f.Random.Bool());
        Ignore(p => p.Installer);
        Ignore(p => p.Name);
    }
}