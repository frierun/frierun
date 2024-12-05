namespace Frierun.Server.Data;

public class DirectedAcyclicGraph<TKey>
    where TKey : notnull
{
    private readonly HashSet<TKey> _vertices = new();
    private readonly Dictionary<TKey, HashSet<TKey>> _incomingEdges = new();
    private readonly Dictionary<TKey, HashSet<TKey>> _outgoingEdges = new();
    
    /// <summary>
    /// Adds vertex to DAG.
    /// </summary>
    public void AddVertex(TKey vertex)
    {
        _vertices.Add(vertex);
    }
    
    /// <summary>
    /// Adds edge from one vertex to another.
    /// </summary>
    public void AddEdge(TKey from, TKey to)
    {
        if (!_vertices.Contains(from))
        {
            throw new ArgumentException($"Vertex {from} is not in the graph");
        }
        
        if (!_vertices.Contains(to))
        {
            throw new ArgumentException($"Vertex {to} is not in the graph");
        }

        if (!_incomingEdges.TryGetValue(to, out var edges))
        {
            _incomingEdges[to] = [from];
        }
        else
        {
            edges.Add(from);
        }
        
        if (!_outgoingEdges.TryGetValue(from, out edges))
        {
            _outgoingEdges[from] = [to];
        }
        else
        {
            edges.Add(to);
        }
    }
    
    /// <summary>
    /// Recursively runs delegate on all verticies using Depth First Search.
    /// </summary>
    public void RunDfs(Action<TKey> action)
    {
        var visited = new HashSet<TKey>();

        foreach (var vertex in _vertices)
        {
            RunDfs(action, vertex, visited);
        }
    }

    /// <summary>
    /// Recursively runs delegate on all contracts in respect of dependencies.
    /// </summary>
    private void RunDfs(
        Action<TKey> action,
        TKey vertex,
        HashSet<TKey> visited
    )
    {
        if (!visited.Add(vertex))
        {
            return;
        }
        
        if (_incomingEdges.TryGetValue(vertex, out var incomingEdges))
        {
            foreach (var edge in incomingEdges )
            {
                RunDfs(action, edge, visited);
            }
        }
        
        action(vertex);
    }
    
    /// <summary>
    /// Gets all direct prerequisites for a vertex.
    /// </summary>
    public IEnumerable<TKey> GetPrerequisites(TKey vertex)
    {
        if (!_incomingEdges.TryGetValue(vertex, out var prerequisites))
        {
            return Array.Empty<TKey>();
        }

        return prerequisites;
    }
}