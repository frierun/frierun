namespace Frierun.Server.Models;

public class State
{
    public IList<Application> Applications { get; } = new List<Application>();
}