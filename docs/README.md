# LemonNet

C# wrapper for the LEMON (Library for Efficient Modeling and Optimization in Networks) graph library, providing high-performance network flow algorithms.

## Overview

LemonNet provides a clean, idiomatic C# API for LEMON's graph algorithms. The architecture closely mirrors LEMON's C++ design with separate graph, property map, and algorithm classes.

## Features

- **Separated Architecture**: Graph structure, arc properties, and algorithms are independent components
- **Type-Safe API**: Lightweight `Node` and `Arc` value types prevent mixing incompatible graph elements  
- **High Performance**: Direct P/Invoke to native LEMON C++ implementation
- **Memory Efficient**: Automatic resource management with `IDisposable` pattern

## Current Status

### Implemented
- ✅ `LemonDigraph` - Directed graph data structure
- ✅ `ArcMap` - Arc property storage (supports `long` integer values)
- ✅ `EdmondsKarp` - Maximum flow algorithm
- ✅ `Preflow` - Push-relabel maximum flow algorithm

### Planned
- 🔲 `NetworkSimplex` - Minimum cost flow algorithm  
- 🔲 `CostScaling` - Cost scaling minimum cost flow algorithm
- 🔲 `CapacityScaling` - Capacity scaling algorithm
- 🔲 `CycleCanceling` - Cycle canceling algorithm
- 🔲 Generic `ArcMap<T>` - Support for different value types

## Quick Start

```csharp
using LemonNet;

// Create a directed graph
using var graph = new LemonDigraph();

// Add nodes
Node source = graph.AddNode();
Node sink = graph.AddNode();
Node n1 = graph.AddNode();

// Add arcs and set capacities
using var capacity = new ArcMap(graph);
capacity[graph.AddArc(source, n1)] = 10.0;
capacity[graph.AddArc(n1, sink)] = 5.0;
capacity[graph.AddArc(source, sink)] = 3.0;

// Run maximum flow algorithm
var edmonds = new EdmondsKarp(graph, capacity);
var result = edmonds.Run(source, sink);

Console.WriteLine($"Max flow: {result.MaxFlowValue}");
```

## Architecture

The library follows a three-layer architecture:

1. **LEMON C++ Library** - Original template-based graph algorithms
2. **C Wrapper Layer** - Exposes C++ functionality through C ABI
3. **C# Managed Layer** - Type-safe .NET API using P/Invoke

### Key Design Decisions

- **Separation of Concerns**: Graphs, properties, and algorithms are separate classes
- **Value Semantics**: `Node` and `Arc` are lightweight structs containing only IDs
- **Resource Management**: Graph and ArcMap classes implement `IDisposable`
- **Direct Correspondence**: C# types directly map to LEMON C++ concepts

## Building

### Windows (Visual Studio)
```cmd
msbuild LemonNet.sln /p:Configuration=Release /p:Platform=x64
```

### Linux/macOS
```bash
# Build native library
./build.sh

# Build C# projects
dotnet build src/LemonNet/LemonNet.csproj
dotnet build tests/LemonNet.Tests/LemonNet.Tests.csproj
```

## Testing

The project includes a comprehensive XUnit test suite with 68 tests covering all components. All tests pass on Windows x64.

```bash
# Run all tests
dotnet test tests/LemonNet.Tests/LemonNet.Tests.csproj

# Run with detailed output
dotnet test tests/LemonNet.Tests/LemonNet.Tests.csproj --logger "console;verbosity=detailed"
```

Test coverage includes:
- Graph operations (node/arc management) - 15 tests
- Algorithm correctness (Edmonds-Karp) - 13 tests  
- Algorithm correctness (Preflow) - 15 tests
- Arc property maps - 12 tests
- Parallel arc support - 13 tests
- Error handling and edge cases
- Performance benchmarks

## Requirements

- .NET 9.0 SDK
- C++11 compatible compiler
- Windows: Visual Studio 2022+
- Linux/macOS: GCC or Clang

## Documentation

- [API Reference](API.md) - Detailed class and method documentation
- [Development Guide](DEVELOPMENT.md) - Building and contributing
- [Examples](examples/) - Sample applications

## License

This wrapper is provided under the MIT License. LEMON itself is licensed under the Boost Software License.

## Contributing

Contributions are welcome! Please see [DEVELOPMENT.md](DEVELOPMENT.md) for build instructions and guidelines.