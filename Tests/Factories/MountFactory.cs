using Bogus;
using Frierun.Server.Data;
using Mount = Frierun.Server.Data.Mount;

namespace Frierun.Tests.Factories;

public sealed class MountFactory: Faker<Mount>
{
    public MountFactory(Faker<Volume> volumeFactory, Faker<Container> containerFactory)
    {
        CustomInstantiator(_ => new Mount(""));
        RuleFor(p => p.Path, f => f.System.DirectoryPath());
        RuleFor(p => p.Volume, _ => volumeFactory.Generate().Id);
        RuleFor(p => p.Container, f => containerFactory.Generate().Id);
        RuleFor(p => p.ReadOnly, f => f.Random.Bool());
    }
}