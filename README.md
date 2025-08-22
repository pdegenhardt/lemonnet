# LemonNet - LEMON Graph Library C# Wrapper

[![NuGet](https://img.shields.io/nuget/v/LemonNet.svg)](https://www.nuget.org/packages/LemonNet/)
[![Build Status](https://github.com/pdegenhardt/lemonnet/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/pdegenhardt/lemonnet/actions)

A high-performance C# wrapper for the LEMON (Library for Efficient Modeling and Optimization in Networks) graph library, providing access to state-of-the-art graph algorithms for maximum flow and shortest path problems.

## Installation

```bash
dotnet add package LemonNet
```

**Important:** LemonNet requires x64 architecture. Ensure your project targets x64:
```xml
<PropertyGroup>
  <PlatformTarget>x64</PlatformTarget>
</PropertyGroup>
```

## Features

### Maximum Flow Algorithms
- **Edmonds-Karp**: Classic BFS-based algorithm (O(VE²))
- **Preflow**: Push-relabel algorithm, generally faster (O(V²√E))

### Shortest Path Algorithms
- **Dijkstra**: Single-source shortest path for non-negative weights (O((V+E)logV))
- **Bellman-Ford**: Handles negative weights and detects negative cycles (O(VE))

### Core Features
- **Full Parallel Arc Support**: Handle multiple edges between the same nodes
- **Type-Safe API**: Strongly-typed Node, Arc, and Path structures
- **High Performance**: Native C++ performance with minimal marshaling overhead
- **Memory Efficient**: Uses value types and unsafe spans for zero-copy operations
- **Flexible Arc Maps**: Support for both integer and floating-point edge weights

## Quick Start

### Maximum Flow Example

```csharp
using LemonNet;

// Create a directed graph
using var graph = new LemonDigraph();

// Add nodes
var source = graph.AddNode();
var v1 = graph.AddNode();
var v2 = graph.AddNode();
var sink = graph.AddNode();

// Add arcs with capacities
var arc01 = graph.AddArc(source, v1);
var arc02 = graph.AddArc(source, v2);
var arc13 = graph.AddArc(v1, sink);
var arc23 = graph.AddArc(v2, sink);

// Solve with Edmonds-Karp
using var edmondsKarp = new EdmondsKarp(graph);
edmondsKarp.SetCapacity(arc01, 16);
edmondsKarp.SetCapacity(arc02, 13);
edmondsKarp.SetCapacity(arc13, 12);
edmondsKarp.SetCapacity(arc23, 20);

var result = edmondsKarp.Run(source, sink);
Console.WriteLine($"Max flow: {result.MaxFlowValue}");
```

### Shortest Path Example

```csharp
using LemonNet;

// Create a directed graph
using var graph = new LemonDigraph();

// Add nodes
var nodeA = graph.AddNode();
var nodeB = graph.AddNode();
var nodeC = graph.AddNode();
var nodeD = graph.AddNode();

// Add arcs with distances
var arcAB = graph.AddArc(nodeA, nodeB);
var arcAC = graph.AddArc(nodeA, nodeC);
var arcBD = graph.AddArc(nodeB, nodeD);
var arcCD = graph.AddArc(nodeC, nodeD);

// Set arc lengths
using var lengthMap = new ArcMapDouble(graph);
lengthMap[arcAB] = 4.0;
lengthMap[arcAC] = 2.0;
lengthMap[arcBD] = 5.0;
lengthMap[arcCD] = 1.0;

// Find shortest path with Dijkstra
using var dijkstra = new Dijkstra(graph, lengthMap);
var result = dijkstra.Run(nodeA, nodeD);

Console.WriteLine($"Shortest distance: {result.Distance}");
Console.WriteLine($"Path length: {result.Path?.Length ?? 0} arcs");

// For graphs with negative weights, use Bellman-Ford
using var bellmanFord = new BellmanFord(graph, lengthMap);
var bfResult = bellmanFord.Run(nodeA, nodeD);
Console.WriteLine($"Has negative cycle: {bfResult.HasNegativeCycle}");
```

## Building

### Prerequisites
- .NET 9.0 SDK or later
- C++14 compiler (GCC, Clang, or Visual Studio 2022+)
- Windows x64, Linux x64, or macOS

### Build Steps

#### Option 1: Visual Studio (Windows)
1. Open `LemonNet.sln` in Visual Studio 2022+
2. Select `Debug|x64` or `Release|x64` configuration
3. Build → Rebuild Solution

#### Option 2: Command Line

**Linux/macOS:**
```bash
# Build native library
./build.sh

# Build C# projects
dotnet build

# Run tests
dotnet test
```

**Windows (Developer Command Prompt):**
```cmd
# Build native library
build.bat

# Build C# projects
dotnet build

# Run tests
dotnet test
```

## API Overview

### Core Types

- **`Node`**: Represents a vertex in the graph (value type)
- **`Arc`**: Represents a directed edge (value type)
- **`LemonDigraph`**: The directed graph container

### Algorithm Classes

- **`EdmondsKarp`**: BFS-based max flow algorithm
- **`Preflow`**: Push-relabel max flow algorithm (usually faster)

### Result Types

- **`MaxFlowResult`**: Contains the maximum flow value and per-edge flows
- **`EdgeFlow`**: Flow value on a specific edge

## Performance

Both algorithms have been optimized for performance:

| Graph Size | Edmonds-Karp | Preflow | Speedup |
|------------|--------------|---------|---------|
| 100 nodes  | ~5ms         | ~2ms    | 2.5x    |
| 1000 nodes | ~200ms       | ~50ms   | 4x      |
| 10000 nodes| ~8000ms      | ~800ms  | 10x     |

*Results vary based on graph structure and density*

## Architecture

The library uses a three-layer architecture:

1. **LEMON C++ Library**: Original template-based graph algorithms
2. **C Wrapper Layer**: Exposes C++ templates through C ABI for P/Invoke
3. **C# Managed Layer**: Type-safe .NET API using P/Invoke

```
┌─────────────────┐
│   C# Client     │
├─────────────────┤
│  LemonNet.dll   │  ← C# Managed API
├─────────────────┤
│lemon_wrapper.dll│  ← C ABI Wrapper
├─────────────────┤
│  LEMON C++ Lib  │  ← Template Implementation
└─────────────────┘
```

## Documentation

- [API Reference](docs/API.md) - Complete API documentation
- [Development Guide](docs/DEVELOPMENT.md) - Building and contributing
- [Architecture Overview](docs/Overview.md) - Design decisions and internals

## Testing

The project includes comprehensive unit tests:

```bash
cd tests/LemonNet.Tests
dotnet test
```

Tests cover:
- Basic graph operations
- Both max flow algorithms
- Edge cases (disconnected graphs, invalid nodes)
- Parallel arcs
- Performance benchmarks
- Cross-validation between algorithms

## License

This wrapper is provided as-is for use with the LEMON library. Please refer to the LEMON library license for terms of use.

## Contributing

Contributions are welcome! Please ensure:
- All tests pass
- New features include tests
- API changes are documented
- Performance is not degraded

## Acknowledgments

This project wraps the [LEMON Graph Library](https://lemon.cs.elte.hu/) developed by the Egerváry Research Group on Combinatorial Optimization (EGRES).