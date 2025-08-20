# LemonNet - LEMON Graph Library C# Wrapper

A high-performance C# wrapper for the LEMON (Library for Efficient Modeling and Optimization in Networks) graph library, providing access to state-of-the-art maximum flow algorithms.

## Features

- **Multiple Maximum Flow Algorithms**:
  - **Edmonds-Karp**: Classic BFS-based algorithm (O(VE²))
  - **Preflow**: Push-relabel algorithm, generally faster (O(V²√E))
- **Full Parallel Arc Support**: Handle multiple edges between the same nodes
- **Type-Safe API**: Strongly-typed Node and Arc structures
- **High Performance**: Native C++ performance with minimal marshaling overhead
- **Memory Efficient**: Uses value types and unsafe spans for zero-copy operations

## Quick Start

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
var capacity = new ArcMap(graph);
capacity[graph.AddArc(source, v1)] = 16.0;
capacity[graph.AddArc(source, v2)] = 13.0;
capacity[graph.AddArc(v1, sink)] = 12.0;
capacity[graph.AddArc(v2, sink)] = 20.0;
capacity[graph.AddArc(v1, v2)] = 10.0;

// Solve with Edmonds-Karp
var ek = new EdmondsKarp(graph, capacity);
var result1 = ek.Run(source, sink);
Console.WriteLine($"Max flow (Edmonds-Karp): {result1.MaxFlowValue}");

// Solve with Preflow (generally faster)
using var preflow = new Preflow(graph, capacity);
var result2 = preflow.Run(source, sink);
Console.WriteLine($"Max flow (Preflow): {result2.MaxFlowValue}");
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
- **`ArcMap`**: Associates capacities with arcs

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