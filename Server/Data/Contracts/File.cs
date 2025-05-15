using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record File(
    string Path,
    string? Name = null,
    string? Text = null,
    string? VolumeName = null,
    int? Owner = null,
    int? Group = null
) : Contract(Name ?? $"{Path}{(VolumeName != null ? " in " + VolumeName : "")}"), IHasStrings
{
    public string VolumeName { get; } = VolumeName ?? "";
    [JsonIgnore] public ContractId<Volume> VolumeId => new(VolumeName);
    
    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Text = Text == null ? null : decorator(Text),
        };
    }    
}