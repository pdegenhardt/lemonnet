# LemonNet API Reference

## LemonMaxFlow Class

The main class for creating and solving maximum flow problems using the Edmonds-Karp algorithm.

### Constructor

```csharp
public LemonMaxFlow()
```

Creates a new instance of the max flow solver with an empty graph.

**Throws:**
- `InvalidOperationException` - If the native library cannot be loaded

### Methods

#### AddNode

```csharp
public int AddNode()
```

Adds a new node to the graph.

**Returns:**
- `int` - The ID of the newly created node (0-based sequential)

**Throws:**
- `ObjectDisposedException` - If the object has been disposed
- `InvalidOperationException` - If the node cannot be added

#### AddEdge

```csharp
public void AddEdge(int source, int target, double capacity)
```

Adds a directed edge from source to target with the specified capacity.

**Parameters:**
- `source` - The source node ID
- `target` - The target node ID  
- `capacity` - The maximum flow capacity of the edge (must be non-negative)

**Throws:**
- `ObjectDisposedException` - If the object has been disposed
- `ArgumentException` - If source or target node doesn't exist, or capacity is negative

#### ComputeMaxFlow

```csharp
public MaxFlowResult ComputeMaxFlow(int source, int target)
```

Computes the maximum flow from source to target using the Edmonds-Karp algorithm.

**Parameters:**
- `source` - The source node ID
- `target` - The target (sink) node ID

**Returns:**
- `MaxFlowResult` - Object containing the maximum flow value and edge flows

**Throws:**
- `ObjectDisposedException` - If the object has been disposed
- `ArgumentException` - If source or target node doesn't exist
- `InvalidOperationException` - If the computation fails

#### Dispose

```csharp
public void Dispose()
```

Releases all native resources. After disposal, the object cannot be used.

### Properties

None (all state is encapsulated)

## MaxFlowResult Class

Immutable class representing the result of a maximum flow computation.

### Properties

#### MaxFlowValue

```csharp
public double MaxFlowValue { get; }
```

The total maximum flow from source to sink.

#### EdgeFlows

```csharp
public List<EdgeFlow> EdgeFlows { get; }
```

List of all edges that carry positive flow in the solution.

### Methods

#### ToString

```csharp
public override string ToString()
```

Returns a string representation of the result.

**Returns:**
- String in format: "Max Flow: {value}, Edges with flow: {count}"

## EdgeFlow Class

Immutable class representing flow on a single edge.

### Properties

#### Source

```csharp
public int Source { get; }
```

The source node ID of the edge.

#### Target

```csharp
public int Target { get; }
```

The target node ID of the edge.

#### Flow

```csharp
public double Flow { get; }
```

The amount of flow on this edge in the maximum flow solution.

### Constructor

```csharp
public EdgeFlow(int source, int target, double flow)
```

Creates a new edge flow instance.

**Parameters:**
- `source` - Source node ID
- `target` - Target node ID
- `flow` - Flow value

### Methods

#### ToString

```csharp
public override string ToString()
```

Returns a string representation of the edge flow.

**Returns:**
- String in format: "Edge({source} -> {target}): Flow = {flow}"

## Usage Examples

### Simple Network

```csharp
using LemonNet;

using (var maxFlow = new LemonMaxFlow())
{
    // Create a simple network
    int s = maxFlow.AddNode();  // Source
    int t = maxFlow.AddNode();  // Sink
    
    // Add edge with capacity 10
    maxFlow.AddEdge(s, t, 10.0);
    
    // Compute max flow
    var result = maxFlow.ComputeMaxFlow(s, t);
    Console.WriteLine($"Max flow: {result.MaxFlowValue}"); // Output: 10
}
```

### Complex Network

```csharp
using LemonNet;

using (var maxFlow = new LemonMaxFlow())
{
    // Create nodes
    int source = maxFlow.AddNode();
    int a = maxFlow.AddNode();
    int b = maxFlow.AddNode();
    int c = maxFlow.AddNode();
    int sink = maxFlow.AddNode();
    
    // Create network
    maxFlow.AddEdge(source, a, 10);
    maxFlow.AddEdge(source, b, 10);
    maxFlow.AddEdge(a, b, 2);
    maxFlow.AddEdge(a, c, 4);
    maxFlow.AddEdge(a, sink, 8);
    maxFlow.AddEdge(b, c, 9);
    maxFlow.AddEdge(c, sink, 10);
    
    // Solve
    var result = maxFlow.ComputeMaxFlow(source, sink);
    
    Console.WriteLine($"Maximum flow: {result.MaxFlowValue}");
    Console.WriteLine("Edge flows:");
    foreach (var edge in result.EdgeFlows)
    {
        Console.WriteLine($"  {edge}");
    }
}
```

### Error Handling

```csharp
using LemonNet;

try
{
    using (var maxFlow = new LemonMaxFlow())
    {
        int node1 = maxFlow.AddNode();
        int node2 = maxFlow.AddNode();
        
        // This will throw - negative capacity
        maxFlow.AddEdge(node1, node2, -5.0);
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Thread Safety

- `LemonMaxFlow` is **not** thread-safe. Each thread should use its own instance.
- `MaxFlowResult` and `EdgeFlow` are immutable and thread-safe.

## Performance Characteristics

- **Time Complexity**: O(V * E²) where V is vertices and E is edges
- **Space Complexity**: O(V + E)
- **Node IDs**: Sequential integers starting from 0
- **Maximum Nodes**: Limited by available memory

## Native Interop Details

The library uses P/Invoke to call the native LEMON library:

```csharp
[DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
private static extern IntPtr lemon_create_graph();
```

The native library must be present as `lemon_wrapper.dll` (Windows) in the same directory as the executable or in the system PATH.

## Disposal Pattern

`LemonMaxFlow` implements `IDisposable` to ensure native memory is freed:

```csharp
// Recommended usage with using statement
using (var maxFlow = new LemonMaxFlow())
{
    // Use maxFlow
} // Automatically disposed

// Manual disposal
var maxFlow = new LemonMaxFlow();
try
{
    // Use maxFlow
}
finally
{
    maxFlow.Dispose();
}
```