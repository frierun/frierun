namespace Frierun.Server.Data;

public record VolumeContract(string Name) : Contract<Volume>(Name);