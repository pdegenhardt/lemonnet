using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LemonNet;

/// <summary>
/// Bellman-Ford shortest path algorithm implementation.
/// Finds shortest paths from a source node to other nodes in a digraph.
/// Supports negative arc lengths and can detect negative cycles.
/// </summary>
public class BellmanFord : IDisposable
{
    private readonly LemonDigraph graph;
    private readonly ArcMapDouble lengthMap;
    private bool disposed = false;

    #region P/Invoke declarations

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePathResult
    {
        public IntPtr arc_ids;
        public int count;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeShortestPathResult
    {
        public double distance;
        public IntPtr path;
        public int reached;
        public int negative_cycle;
    }

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr lemon_bellman_ford(IntPtr graph, IntPtr length_map, int source, int target);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_free_shortest_path_result(IntPtr result);

    #endregion

    /// <summary>
    /// Creates a new instance of the Bellman-Ford algorithm.
    /// </summary>
    /// <param name="graph">The digraph to operate on.</param>
    /// <param name="lengthMap">The arc map containing arc lengths (can be negative).</param>
    public BellmanFord(LemonDigraph graph, ArcMapDouble lengthMap)
    {
        this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
        this.lengthMap = lengthMap ?? throw new ArgumentNullException(nameof(lengthMap));
    }

    /// <summary>
    /// Runs the Bellman-Ford algorithm from the source to the target node.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <param name="target">The target node.</param>
    /// <returns>The shortest path result.</returns>
    public ShortestPathResult Run(Node source, Node target)
    {
        ThrowIfDisposed();

        if (!source.IsValid)
            throw new ArgumentException("Invalid source node", nameof(source));
        if (!target.IsValid)
            throw new ArgumentException("Invalid target node", nameof(target));

        IntPtr resultPtr = lemon_bellman_ford(graph.Handle, lengthMap.Handle, source.Id, target.Id);
        
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to compute shortest path");
        }

        try
        {
            NativeShortestPathResult nativeResult = Marshal.PtrToStructure<NativeShortestPathResult>(resultPtr);
            
            bool hasNegativeCycle = nativeResult.negative_cycle != 0;
            bool reached = nativeResult.reached != 0;
            Path? path = null;

            if (hasNegativeCycle)
            {
                return ShortestPathResult.NegativeCycle();
            }

            if (reached && nativeResult.path != IntPtr.Zero)
            {
                NativePathResult nativePath = Marshal.PtrToStructure<NativePathResult>(nativeResult.path);
                
                if (nativePath.count > 0 && nativePath.arc_ids != IntPtr.Zero)
                {
                    int[] arcIds = new int[nativePath.count];
                    Marshal.Copy(nativePath.arc_ids, arcIds, 0, nativePath.count);
                    
                    Arc[] arcs = new Arc[nativePath.count];
                    for (int i = 0; i < nativePath.count; i++)
                    {
                        arcs[i] = new Arc(arcIds[i]);
                    }
                    
                    path = new Path(graph, arcs);
                }
                else
                {
                    path = new Path(graph);
                }
            }

            if (!reached)
            {
                return ShortestPathResult.Unreachable();
            }

            return new ShortestPathResult(nativeResult.distance, path, reached, false);
        }
        finally
        {
            lemon_free_shortest_path_result(resultPtr);
        }
    }

    /// <summary>
    /// Finds the shortest path from source to target.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <param name="target">The target node.</param>
    /// <returns>The shortest path, or null if no path exists or a negative cycle is detected.</returns>
    public Path FindPath(Node source, Node target)
    {
        var result = Run(source, target);
        return result.Path;
    }

    /// <summary>
    /// Finds the shortest distance from source to target.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <param name="target">The target node.</param>
    /// <returns>The shortest distance, or double.PositiveInfinity if no path exists or a negative cycle is detected.</returns>
    public double FindDistance(Node source, Node target)
    {
        var result = Run(source, target);
        return result.Distance;
    }

    /// <summary>
    /// Checks if there is a negative cycle reachable from the source node.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <returns>True if a negative cycle is detected, false otherwise.</returns>
    public bool HasNegativeCycle(Node source)
    {
        // Run to any target to detect negative cycles
        // We use the source as target since we just need to run the algorithm
        var result = Run(source, source);
        return result.HasNegativeCycle;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}