# LemonNet - C# Wrapper for LEMON Graph Library

## Overview

LemonNet provides a managed C# wrapper for the LEMON (Library for Efficient Modeling and Optimization in Networks) graph library's Edmonds-Karp maximum flow algorithm. This project demonstrates how to successfully integrate a complex C++ template library with .NET through a clean, type-safe API.

## What We Achieved

### 1. Native C++ to C# Integration
- Created a C wrapper layer (`lemon_wrapper.cpp/h`) that exposes LEMON's templated C++ code through a C ABI
- Implemented P/Invoke marshaling to safely call native code from C#
- Handled memory management across managed/unmanaged boundaries
- Resolved complex linking issues with C++ template instantiations

### 2. Clean Object-Oriented API
- Wrapped low-level P/Invoke calls in a user-friendly C# class (`LemonMaxFlow`)
- Implemented `IDisposable` pattern for proper resource cleanup
- Provided type-safe methods for graph construction and flow computation
- Created immutable result objects for thread safety

### 3. Professional Project Structure
```
LemonNet/
├── src/
│   ├── LemonNet/              # C# library project
│   │   ├── LemonMaxFlow.cs    # Main wrapper class
│   │   └── runtimes/           # Platform-specific native binaries
│   │       └── win-x64/native/
│   └── LemonNet.Native/        # C++ wrapper project
│       ├── lemon_wrapper.cpp  # C wrapper implementation
│       └── lemon-1.3.1/        # LEMON headers
├── tests/
│   └── LemonNet.Tests/         # Test console application
└── docs/                       # Documentation
```

### 4. Visual Studio Integration
- Full MSBuild integration with automatic dependency management
- Native C++ project builds automatically trigger before C# compilation
- Proper x64 platform configuration throughout
- Debugging support for both managed and native code

### 5. Automatic Native Binary Management
- Native DLL automatically copies to output directories
- Proper runtime folder structure for NuGet packaging
- Platform-specific binary selection (x64)
- No manual file copying required

## Technical Challenges Solved

### Static Member Initialization
LEMON uses static template members that must be explicitly instantiated. We solved undefined symbol errors by:
```cpp
namespace lemon {
    float Tolerance<float>::def_epsilon = static_cast<float>(1e-4);
    double Tolerance<double>::def_epsilon = 1e-10;
    long double Tolerance<long double>::def_epsilon = 1e-14;
    const Invalid INVALID = Invalid();
}
```

### Windows Platform Support
Integrated Windows-specific threading primitives by including `lemon/bits/windows.cc` and defining proper preprocessor macros (`WIN32`, `_WINDOWS`).

### Platform Architecture Alignment
Ensured x64 consistency across all projects to avoid `BadImageFormatException`:
- Native library: x64 only
- C# projects: `<PlatformTarget>x64</PlatformTarget>`
- Solution configuration: Debug|x64 and Release|x64

### Build Order Dependencies
Used MSBuild project references to ensure correct build order:
```xml
<ProjectReference Include="..\LemonNet.Native\LemonWrapper.Native.vcxproj">
  <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
</ProjectReference>
```

## Usage Example

```csharp
using LemonNet;

using (var maxFlow = new LemonMaxFlow())
{
    // Add nodes
    int source = maxFlow.AddNode();
    int node1 = maxFlow.AddNode();
    int node2 = maxFlow.AddNode();
    int sink = maxFlow.AddNode();
    
    // Add edges with capacities
    maxFlow.AddEdge(source, node1, 20.0);
    maxFlow.AddEdge(node1, node2, 10.0);
    maxFlow.AddEdge(node1, sink, 10.0);
    maxFlow.AddEdge(node2, sink, 20.0);
    
    // Compute maximum flow
    var result = maxFlow.ComputeMaxFlow(source, sink);
    
    Console.WriteLine($"Maximum flow: {result.MaxFlowValue}");
    foreach (var edge in result.EdgeFlows)
    {
        Console.WriteLine($"Flow on edge {edge.Source} -> {edge.Target}: {edge.Flow}");
    }
}
```

## Key Design Decisions

### Why P/Invoke Instead of C++/CLI
- **Cross-platform compatibility**: P/Invoke works with .NET Core/5/6/7/8/9
- **Simplicity**: No mixed-mode assemblies or complex build configurations
- **Performance**: Minimal overhead for the scale of operations
- **Maintainability**: Clear separation between native and managed code

### Why C Wrapper Layer
- **ABI stability**: C++ name mangling varies between compilers
- **Template instantiation**: LEMON uses heavy templating that can't be directly P/Invoked
- **Memory management**: Provides a clear boundary for resource ownership

### Why x64 Only
- **Modern standard**: 32-bit applications are essentially obsolete
- **Simplicity**: Eliminates platform mismatch errors
- **Performance**: Better memory addressing for large graphs

## Building the Project

### Prerequisites
- Visual Studio 2022 with C++ development workload
- .NET 9.0 SDK
- Windows SDK

### Build Steps
1. Open `LemonNet.sln` in Visual Studio
2. Select `Debug|x64` or `Release|x64` configuration
3. Build → Rebuild Solution
4. Run tests with F5

The build process automatically:
- Compiles the C++ wrapper to `lemon_wrapper.dll`
- Copies the DLL to the appropriate runtime folder
- Builds the C# library with embedded native references
- Runs the test application

## Future Enhancements

### Additional Algorithms
The wrapper architecture can be extended to expose more LEMON algorithms:
- Preflow push-relabel maximum flow
- Minimum cost flow algorithms
- Shortest path algorithms (Dijkstra, Bellman-Ford)
- Minimum spanning tree algorithms

### Cross-Platform Support
The project structure supports adding:
- Linux binaries (`runtimes/linux-x64/native/lemon_wrapper.so`)
- macOS binaries (`runtimes/osx-x64/native/lemon_wrapper.dylib`)

### NuGet Package
The project is configured for NuGet packaging:
```xml
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
<PackageId>LemonNet</PackageId>
```

## Lessons Learned

1. **Start simple**: Begin with a minimal working example before adding complexity
2. **Platform consistency**: Ensure all components target the same architecture
3. **Automatic is better**: Use MSBuild features to automate file copying and dependencies
4. **Clear boundaries**: Separate native and managed code cleanly
5. **Test early**: Verify native loading before implementing features

## Conclusion

This project successfully demonstrates how to wrap a complex C++ template library for use in C#. The resulting API is clean, type-safe, and performs well while hiding all the complexity of native interop from the end user. The solution integrates seamlessly with Visual Studio and modern .NET development workflows.