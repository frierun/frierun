namespace Frierun.Server.Data;

public record Parameter(
    string Name,
    string? DefaultValue = null,
    string? Value = null
) : Contract(Name)
{
    public override Contract With(Contract other)
    {
        if (other is not Parameter parameter || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }

        return this with
        {
            Value = parameter.Value ?? Value,
            DefaultValue = parameter.DefaultValue ?? DefaultValue
        };
    }
}