# Performance Benchmarks

This document presents performance benchmarks comparing the Edmonds-Karp and Preflow maximum flow algorithms implemented in LemonNet.

## Benchmark Environment

- **Hardware**: 11th Gen Intel Core i7-11370H 3.30GHz, 1 CPU, 8 logical and 4 physical cores
- **OS**: Windows 11 (10.0.26100.4946)
- **Runtime**: .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
- **Benchmark Tool**: BenchmarkDotNet v0.14.0

## Graph Structures

All benchmarks use layered flow networks with the following characteristics:

### Small Graph (~1,000 arcs)
- **Structure**: 5 layers (Source → 20 → 30 → 20 → Sink)
- **Total nodes**: 72
- **Total arcs**: ~1,000
- **Capacities**: 5-100

### Large Graph (~100,000 arcs)
- **Structure**: 6 layers (Source → 100 → 200 → 300 → 200 → 100 → Sink)
- **Total nodes**: 902
- **Total arcs**: ~100,000
- **Capacities**: 50-1000

### Extra Large Graph (~500,000 arcs)
- **Structure**: 7 layers (Source → 200 → 400 → 600 → 600 → 400 → 200 → Sink)
- **Total nodes**: 2,402
- **Total arcs**: ~500,000
- **Capacities**: 50-1000

## Benchmark Results

| Graph Size | Algorithm | Mean Time | Memory Allocated | Speedup |
|------------|-----------|-----------|------------------|---------|
| ~1K arcs | Edmonds-Karp | 203.49 μs | 2.41 KB | - |
| ~1K arcs | Preflow | 17.45 μs | 2.29 KB | **12x** |
| ~100K arcs | Edmonds-Karp | 167,688 μs (168 ms) | 21.71 KB | - |
| ~100K arcs | Preflow | 544 μs | 21.04 KB | **308x** |
| ~500K arcs | Edmonds-Karp | 2,443,336 μs (2.44 s) | 56.07 KB | - |
| ~500K arcs | Preflow | 2,879 μs (2.9 ms) | 53.67 KB | **848x** |

## Key Findings

### Performance Scaling
The performance gap between algorithms widens dramatically with graph size:
- Small graphs: Preflow is 12x faster
- Medium graphs: Preflow is 308x faster
- Large graphs: Preflow is 848x faster

### Algorithm Complexity in Practice
These results demonstrate the theoretical complexity differences:
- **Edmonds-Karp**: O(VE²) - quadratic in the number of edges
- **Preflow**: O(V²E) or better - much more efficient for dense graphs

### Memory Efficiency
Both algorithms demonstrate excellent memory efficiency:
- Memory usage scales linearly with graph size
- Less than 60 KB for graphs with 500,000 arcs
- Minimal garbage collection pressure

### Practical Implications

1. **Small Graphs (<10K arcs)**: Either algorithm is suitable, though Preflow still offers better performance
2. **Medium Graphs (10K-100K arcs)**: Preflow is strongly recommended for responsive applications
3. **Large Graphs (>100K arcs)**: Preflow is essential - Edmonds-Karp becomes impractical

### Use Case Recommendations

- **Interactive Applications**: Use Preflow for any graph over 1,000 arcs
- **Batch Processing**: Edmonds-Karp may be acceptable for small graphs if simplicity is valued
- **Real-time Systems**: Always use Preflow regardless of graph size
- **Network Flow Analysis**: Preflow is the clear choice for production systems

## Running the Benchmarks

To reproduce these benchmarks:

```bash
cd examples/LemonNet.Benchmark
dotnet run -c Release
```

The benchmark suite includes:
- `BenchmarkEdmondsKarp` - Small graph
- `BenchmarkPreflow` - Small graph
- `BenchmarkEdmondsKarpLarge` - Large graph (~100K arcs)
- `BenchmarkPreflowLarge` - Large graph (~100K arcs)
- `BenchmarkEdmondsKarpExtraLarge` - Extra large graph (~500K arcs)
- `BenchmarkPreflowExtraLarge` - Extra large graph (~500K arcs)

## Conclusion

The Preflow algorithm demonstrates superior performance across all graph sizes, with the advantage becoming more pronounced as graphs grow larger. For production applications dealing with non-trivial graph sizes, Preflow is the recommended choice.