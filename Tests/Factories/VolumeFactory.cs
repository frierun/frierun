using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public sealed class VolumeFactory: ContractFaker<Volume>
{
    public VolumeFactory()
    {
        CustomInstantiator(_ => new Volume());
    }
}