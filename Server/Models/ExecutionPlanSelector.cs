using System.Text.Json.Serialization;

namespace Frierun.Server.Models;

public class ExecutionPlanSelector
{
    public IList<ExecutionPlan> Children { get; } = new List<ExecutionPlan>();
    public int SelectedIndex { get; set; }
    
    [JsonIgnore]
    public ExecutionPlan Selected => Children[SelectedIndex];
    
}