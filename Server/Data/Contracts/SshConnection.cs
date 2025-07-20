using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record SshConnection(
    string? Name = null,
    string? Host = null,
    int Port = 0,
    string? Username = null,
    string? Password = null
)
    : Contract(Name ?? ""), IHasStrings
{
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Host = OnlyOne(Host, contract.Host),
            Port = OnlyOne(Port, contract.Port, port => port == 0),
            Username = OnlyOne(Username, contract.Username),
            Password = OnlyOne(Password, contract.Password)
        };
    }

    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Host = Host == null ? null : decorator(Host),
            Username = Username == null ? null : decorator(Username),
            Password = Password == null ? null : decorator(Password)
        };
    }
}