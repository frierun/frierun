namespace Frierun.Server.Data;

public record Password(
    string? Name = "",
    GeneratedPassword? Result = null
) : Contract(Name ?? ""), IHasResult<GeneratedPassword>;