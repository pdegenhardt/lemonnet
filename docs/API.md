# LemonNet API Reference

## Core Types

### `Node` (struct)
Represents a node in a directed graph. This is a lightweight value type containing only an ID.

```csharp
public struct Node : IEquatable<Node>
{
    public static readonly Node Invalid; // Represents an invalid node (-1)
    public bool IsValid { get; }         // True if node ID >= 0
}
```

**Usage:**
```csharp
Node n1 = graph.AddNode();
if (n1.IsValid) { /* ... */ }
```

### `Arc` (struct)
Represents a directed edge in the graph. This is a lightweight value type containing only an ID.

```csharp
public struct Arc : IEquatable<Arc>
{
    public static readonly Arc Invalid;  // Represents an invalid arc (-1)
    public bool IsValid { get; }         // True if arc ID >= 0
}
```

**Usage:**
```csharp
Arc arc = graph.AddArc(source, target);
if (arc.IsValid) { /* ... */ }
```

## Graph Classes

### `LemonDigraph`
Represents a directed graph using the LEMON library.

```csharp
public class LemonDigraph : IDisposable
{
    // Properties
    public int NodeCount { get; }
    public int ArcCount { get; }
    
    // Node operations
    public Node AddNode();
    
    // Arc operations
    public Arc AddArc(Node source, Node target);
    public Node Source(Arc arc);
    public Node Target(Arc arc);
    
    // Validation
    public bool IsValid(Node node);
    public bool IsValid(Arc arc);
}
```

**Example:**
```csharp
using var graph = new LemonDigraph();

// Build graph
Node n1 = graph.AddNode();
Node n2 = graph.AddNode();
Arc arc = graph.AddArc(n1, n2);

// Query graph
Node source = graph.Source(arc);  // Returns n1
Node target = graph.Target(arc);  // Returns n2
int nodeCount = graph.NodeCount;  // Returns 2
```

### `ArcMap`
Associates values with arcs in a graph. Currently supports `double` values.

```csharp
public class ArcMap : IDisposable
{
    // Constructor
    public ArcMap(LemonDigraph graph);
    
    // Properties
    public LemonDigraph ParentGraph { get; }
    
    // Value access
    public void SetValue(Arc arc, double value);
    public double GetValue(Arc arc);
    public double this[Arc arc] { get; set; }  // Indexer
}
```

**Example:**
```csharp
using var capacity = new ArcMap(graph);
Arc arc = graph.AddArc(n1, n2);

// Set capacity using indexer
capacity[arc] = 10.5;

// Or using method
capacity.SetValue(arc, 10.5);

// Get value
double cap = capacity[arc];
```

## Algorithm Classes

### `EdmondsKarp`
Edmonds-Karp maximum flow algorithm implementation using BFS-based augmenting paths.

```csharp
public class EdmondsKarp
{
    // Constructors
    public EdmondsKarp(LemonDigraph graph, ArcMap capacityMap);
    public EdmondsKarp(LemonDigraph graph);  // Creates new capacity map
    
    // Properties
    public ArcMap CapacityMap { get; }
    
    // Configuration
    public EdmondsKarp SetCapacity(Arc arc, double capacity);
    
    // Algorithm execution
    public MaxFlowResult Run(Node source, Node target);
}
```

**Example:**
```csharp
// Option 1: Use existing capacity map
var capacityMap = new ArcMap(graph);
capacityMap[arc1] = 10.0;
capacityMap[arc2] = 5.0;

var ek = new EdmondsKarp(graph, capacityMap);
var result = ek.Run(source, sink);

// Option 2: Let algorithm manage capacity map
var ek2 = new EdmondsKarp(graph);
ek2.SetCapacity(arc1, 10.0)
   .SetCapacity(arc2, 5.0);
var result2 = ek2.Run(source, sink);
```

### `Preflow`
Preflow push-relabel maximum flow algorithm implementation. Generally faster than Edmonds-Karp with O(n²√m) complexity.

```csharp
public class Preflow : IDisposable
{
    // Constructors
    public Preflow(LemonDigraph graph, ArcMap capacityMap);
    public Preflow(LemonDigraph graph);  // Creates new capacity map
    
    // Properties
    public ArcMap CapacityMap { get; }
    
    // Configuration
    public Preflow SetCapacity(Arc arc, double capacity);
    
    // Algorithm execution
    public MaxFlowResult Run(Node source, Node target);
}
```

**Example:**
```csharp
// Preflow follows the same API as EdmondsKarp
using var preflow = new Preflow(graph, capacityMap);
var result = preflow.Run(source, sink);

// Can be called multiple times with different capacities
preflow.SetCapacity(arc1, 20.0);
var result2 = preflow.Run(source, sink);
```

**Algorithm Comparison:**
- **EdmondsKarp**: O(VE²) complexity, simple BFS-based approach, good for smaller graphs
- **Preflow**: O(V²√E) complexity, push-relabel approach, generally faster on larger graphs

## Result Types

### `MaxFlowResult`
Contains the results of a maximum flow computation.

```csharp
public class MaxFlowResult
{
    public double MaxFlowValue { get; }      // Total flow value
    public List<EdgeFlow> EdgeFlows { get; }  // Flow on each edge
}
```

### `EdgeFlow`
Represents flow on a single edge.

```csharp
public class EdgeFlow
{
    public Node Source { get; }
    public Node Target { get; }
    public double Flow { get; }
}
```

**Example:**
```csharp
var result = edmondsKarp.Run(source, sink);

Console.WriteLine($"Max flow: {result.MaxFlowValue}");

foreach (var flow in result.EdgeFlows)
{
    Console.WriteLine($"Flow from {flow.Source} to {flow.Target}: {flow.Flow}");
}
```

## Complete Example

```csharp
using System;
using LemonNet;

// Create graph
using var graph = new LemonDigraph();

// Add nodes
Node source = graph.AddNode();
Node n1 = graph.AddNode();
Node n2 = graph.AddNode();
Node sink = graph.AddNode();

// Create capacity map
using var capacity = new ArcMap(graph);

// Add arcs with capacities
capacity[graph.AddArc(source, n1)] = 10.0;
capacity[graph.AddArc(source, n2)] = 10.0;
capacity[graph.AddArc(n1, n2)] = 2.0;
capacity[graph.AddArc(n1, sink)] = 4.0;
capacity[graph.AddArc(n2, sink)] = 10.0;

// Run Edmonds-Karp algorithm
var edmonds = new EdmondsKarp(graph, capacity);
var ekResult = edmonds.Run(source, sink);
Console.WriteLine($"Edmonds-Karp max flow: {ekResult.MaxFlowValue}");

// Run Preflow algorithm (generally faster)
using var preflow = new Preflow(graph, capacity);
var pfResult = preflow.Run(source, sink);
Console.WriteLine($"Preflow max flow: {pfResult.MaxFlowValue}");

// Both algorithms return the same max flow value
// Process results
foreach (var flow in pfResult.EdgeFlows)
{
    if (flow.Flow > 0)
    {
        Console.WriteLine($"  {flow.Source} -> {flow.Target}: {flow.Flow}");
    }
}
```

## Memory Management

All classes that wrap native resources implement `IDisposable`:
- `LemonDigraph` - Manages native graph memory
- `ArcMap` - Manages native arc map memory
- `Preflow` - Manages native algorithm state (EdmondsKarp doesn't maintain state)

Always use `using` statements or explicitly call `Dispose()`:

```csharp
using var graph = new LemonDigraph();
using var capacity = new ArcMap(graph);
// ... use graph and capacity ...
// Automatically disposed at end of scope
```

## Parallel Arcs Support

LemonNet fully supports **parallel arcs** (multiple arcs between the same pair of nodes):

```csharp
var graph = new LemonDigraph();
var nodeA = graph.AddNode();
var nodeB = graph.AddNode();

// Create three parallel arcs from A to B
var arc1 = graph.AddArc(nodeA, nodeB);
var arc2 = graph.AddArc(nodeA, nodeB);  // Parallel to arc1
var arc3 = graph.AddArc(nodeA, nodeB);  // Parallel to arc1 and arc2

// Each arc can have different properties
var capacity = new ArcMap(graph);
capacity[arc1] = 10.0;
capacity[arc2] = 5.0;
capacity[arc3] = 15.0;

// Algorithms handle parallel arcs correctly
var edmonds = new EdmondsKarp(graph, capacity);
var result = edmonds.Run(nodeA, nodeB);
// Max flow = 10 + 5 + 15 = 30
```

This is useful for modeling:
- Multiple connections with different characteristics
- Redundant paths in networks
- Different types of flow between same nodes

## Thread Safety

The current implementation is **not thread-safe**. Each thread should use its own graph instances.

## Performance Considerations

- `Node` and `Arc` are value types - copying is cheap
- `EdgeFlow` is a readonly struct - stack allocated, no GC pressure
- Graph operations are O(1) for adding nodes/arcs
- Algorithm complexities:
  - **Edmonds-Karp**: O(VE²) where V = nodes, E = arcs
  - **Preflow**: O(V²√E) - generally faster on larger graphs
- Native memory is managed through P/Invoke - minimal GC pressure
- Result marshaling uses unsafe spans for ~10x performance improvement
- Arrays used instead of Lists where size is known
- Preflow creates native algorithm instance per Run() call for thread safety

## Error Handling

All methods validate inputs and throw appropriate exceptions:
- `ArgumentNullException` - Null parameters
- `ArgumentException` - Invalid nodes/arcs or negative capacities
- `InvalidOperationException` - Algorithm failures
- `ObjectDisposedException` - Using disposed objects