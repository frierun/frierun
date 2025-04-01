namespace Frierun.Server.Installers;

public sealed record InstallerDefinition(
    string TypeName,
    string? ApplicationName = null
);