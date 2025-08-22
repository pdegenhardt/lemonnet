# LemonNet API Documentation

## Core Types

### LemonDigraph
Represents a directed graph (digraph) with nodes and arcs.

```csharp
public class LemonDigraph : IDisposable
{
    // Properties
    public int NodeCount { get; }
    public int ArcCount { get; }
    
    // Methods
    public Node AddNode();
    public Arc AddArc(Node source, Node target);
    public Node Source(Arc arc);
    public Node Target(Arc arc);
}
```

### Node
Represents a node (vertex) in the graph.

```csharp
public struct Node : IEquatable<Node>
{
    public bool IsValid { get; }
    public static readonly Node Invalid;
}
```

### Arc
Represents an arc (directed edge) in the graph.

```csharp
public struct Arc : IEquatable<Arc>
{
    public bool IsValid { get; }
    public static readonly Arc Invalid;
}
```

## Arc Maps

### ArcMap
Maps long integer values to arcs (used for capacities).

```csharp
public class ArcMap : IDisposable
{
    public ArcMap(LemonDigraph graph);
    public void SetValue(Arc arc, long value);
    public long GetValue(Arc arc);
    public long this[Arc arc] { get; set; }
}
```

### ArcMapDouble
Maps double-precision floating-point values to arcs (used for distances/costs).

```csharp
public class ArcMapDouble : IDisposable
{
    public ArcMapDouble(LemonDigraph graph);
    public void SetValue(Arc arc, double value);
    public double GetValue(Arc arc);
    public double this[Arc arc] { get; set; }
}
```

## Maximum Flow Algorithms

### EdmondsKarp
Implements the Edmonds-Karp maximum flow algorithm (BFS-based Ford-Fulkerson).

```csharp
public class EdmondsKarp : IDisposable
{
    public EdmondsKarp(LemonDigraph graph);
    public void SetCapacity(Arc arc, long capacity);
    public MaxFlowResult Run(Node source, Node sink);
}
```

### Preflow
Implements the Preflow (Push-Relabel) maximum flow algorithm.

```csharp
public class Preflow : IDisposable
{
    public Preflow(LemonDigraph graph);
    public void SetCapacity(Arc arc, long capacity);
    public MaxFlowResult Run(Node source, Node sink);
}
```

### MaxFlowResult
Contains the results of a maximum flow computation.

```csharp
public class MaxFlowResult
{
    public long MaxFlowValue { get; }
    public IReadOnlyList<EdgeFlow> EdgeFlows { get; }
}
```

### EdgeFlow
Represents flow on a single arc.

```csharp
public struct EdgeFlow
{
    public Arc Arc { get; }
    public long Flow { get; }
}
```

## Shortest Path Algorithms

### Dijkstra
Implements Dijkstra's shortest path algorithm for graphs with non-negative arc lengths.

```csharp
public class Dijkstra : IDisposable
{
    public Dijkstra(LemonDigraph graph, ArcMapDouble lengthMap);
    public ShortestPathResult Run(Node source, Node target);
    public Path FindPath(Node source, Node target);
    public double FindDistance(Node source, Node target);
}
```

### BellmanFord
Implements the Bellman-Ford shortest path algorithm, supporting negative arc lengths and negative cycle detection.

```csharp
public class BellmanFord : IDisposable
{
    public BellmanFord(LemonDigraph graph, ArcMapDouble lengthMap);
    public ShortestPathResult Run(Node source, Node target);
    public Path FindPath(Node source, Node target);
    public double FindDistance(Node source, Node target);
    public bool HasNegativeCycle(Node source);
}
```

### ShortestPathResult
Contains the results of a shortest path computation.

```csharp
public class ShortestPathResult
{
    public double Distance { get; }
    public Path? Path { get; }
    public bool TargetReached { get; }
    public bool HasNegativeCycle { get; }
    
    // Factory methods
    public static ShortestPathResult Unreachable();
    public static ShortestPathResult NegativeCycle();
}
```

### Path
Represents a path in the graph as a sequence of arcs.

```csharp
public class Path : IEnumerable<Arc>
{
    public Path(LemonDigraph graph, IEnumerable<Arc> arcs);
    
    // Properties
    public int Length { get; }
    public bool IsEmpty { get; }
    public Node Source { get; }
    public Node Target { get; }
    public Arc this[int index] { get; }
    
    // Methods
    public IEnumerable<Node> GetNodes();
    public double GetTotalCost(ArcMapDouble costMap);
}
```

## Usage Examples

### Maximum Flow

```csharp
// Create graph
using var graph = new LemonDigraph();
var source = graph.AddNode();
var sink = graph.AddNode();
var arc = graph.AddArc(source, sink);

// Solve max flow with Edmonds-Karp
using var ek = new EdmondsKarp(graph);
ek.SetCapacity(arc, 100);
var result = ek.Run(source, sink);
Console.WriteLine($"Max flow: {result.MaxFlowValue}");

// Or use Preflow (typically faster)
using var pf = new Preflow(graph);
pf.SetCapacity(arc, 100);
var result2 = pf.Run(source, sink);
```

### Shortest Path

```csharp
// Create graph
using var graph = new LemonDigraph();
var start = graph.AddNode();
var end = graph.AddNode();
var arc = graph.AddArc(start, end);

// Set arc lengths
using var lengths = new ArcMapDouble(graph);
lengths[arc] = 5.0;

// Find shortest path with Dijkstra
using var dijkstra = new Dijkstra(graph, lengths);
var result = dijkstra.Run(start, end);

if (result.TargetReached)
{
    Console.WriteLine($"Distance: {result.Distance}");
    Console.WriteLine($"Path: {result.Path}");
}

// For negative weights, use Bellman-Ford
using var bf = new BellmanFord(graph, lengths);
var bfResult = bf.Run(start, end);

if (bfResult.HasNegativeCycle)
{
    Console.WriteLine("Negative cycle detected!");
}
```

## Performance Characteristics

| Algorithm | Time Complexity | Space Complexity | Notes |
|-----------|----------------|------------------|-------|
| Edmonds-Karp | O(VE²) | O(V + E) | Good for sparse graphs |
| Preflow | O(V²√E) | O(V + E) | Generally faster than Edmonds-Karp |
| Dijkstra | O((V+E)log V) | O(V) | Requires non-negative weights |
| Bellman-Ford | O(VE) | O(V) | Handles negative weights, detects negative cycles |

## Thread Safety

- `LemonDigraph` instances are **not** thread-safe for modifications
- Arc maps are **not** thread-safe
- Algorithm instances should not be shared between threads
- Results (`MaxFlowResult`, `ShortestPathResult`, `Path`) are immutable and thread-safe

## Memory Management

All main classes implement `IDisposable` and should be used with `using` statements:

```csharp
using var graph = new LemonDigraph();
using var arcMap = new ArcMapDouble(graph);
using var algorithm = new Dijkstra(graph, arcMap);
// Resources are automatically cleaned up
```

## Platform Requirements

- **Architecture**: x64 only
- **Runtime**: .NET 9.0 or later
- **OS**: Windows, Linux, or macOS (x64)