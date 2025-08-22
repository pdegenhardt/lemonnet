using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LemonNet;

/// <summary>
/// Dijkstra's shortest path algorithm implementation.
/// Finds shortest paths from a source node to other nodes in a digraph with non-negative arc lengths.
/// </summary>
public class Dijkstra : IDisposable
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
    private static extern IntPtr lemon_dijkstra(IntPtr graph, IntPtr length_map, int source, int target);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_free_shortest_path_result(IntPtr result);

    #endregion

    /// <summary>
    /// Creates a new instance of Dijkstra's algorithm.
    /// </summary>
    /// <param name="graph">The digraph to operate on.</param>
    /// <param name="lengthMap">The arc map containing non-negative arc lengths.</param>
    public Dijkstra(LemonDigraph graph, ArcMapDouble lengthMap)
    {
        this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
        this.lengthMap = lengthMap ?? throw new ArgumentNullException(nameof(lengthMap));
    }

    /// <summary>
    /// Runs Dijkstra's algorithm from the source to the target node.
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

        IntPtr resultPtr = lemon_dijkstra(graph.Handle, lengthMap.Handle, source.Id, target.Id);
        
        if (resultPtr == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to compute shortest path");
        }

        try
        {
            NativeShortestPathResult nativeResult = Marshal.PtrToStructure<NativeShortestPathResult>(resultPtr);
            
            bool reached = nativeResult.reached != 0;
            Path? path = null;

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
    /// <returns>The shortest path, or null if no path exists.</returns>
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
    /// <returns>The shortest distance, or double.PositiveInfinity if no path exists.</returns>
    public double FindDistance(Node source, Node target)
    {
        var result = Run(source, target);
        return result.Distance;
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