# LemonNet Example

This is a simple console application demonstrating how to use the LemonNet NuGet package to solve maximum flow problems.

## Prerequisites

- .NET 9.0 SDK
- x64 architecture (required by LemonNet native libraries)

## Running the Example

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the example:
   ```bash
   dotnet run
   ```

## What It Does

The example creates a simple diamond-shaped graph with 4 nodes and 4 arcs:

```
     1 
    / \
   0   3
    \ /
     2
```

Each arc has a capacity of 10. The program calculates the maximum flow from node 0 (source) to node 3 (sink) using both:
- Edmonds-Karp algorithm
- Preflow (Push-Relabel) algorithm

Expected output: Maximum flow of 20 (10 units through the upper path 0→1→3 and 10 units through the lower path 0→2→3).

## Important Notes

- **Platform Target**: The project file explicitly sets `<PlatformTarget>x64</PlatformTarget>` because LemonNet includes x64-only native libraries.
- **NuGet Package**: The project references the LemonNet package from NuGet.org. Make sure you have access to NuGet.org or configure your local package source if using a local package.

## Troubleshooting

If you get a `DllNotFoundException` or `BadImageFormatException`:
1. Ensure your project is targeting x64 platform
2. Check that the native libraries are being copied to your output directory
3. On Linux, ensure the .so file has execute permissions

If you get a `MissingMethodException`:
1. Ensure you have the latest version of the LemonNet package
2. Clear your NuGet cache: `dotnet nuget locals all --clear`
3. Restore packages again: `dotnet restore`