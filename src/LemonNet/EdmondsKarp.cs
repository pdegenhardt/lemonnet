using System;
using System.Runtime.InteropServices;
using LemonNet.Internal;

namespace LemonNet;

/// <summary>
/// Edmonds-Karp maximum flow algorithm implementation.
/// Uses BFS to find augmenting paths with O(VE²) complexity.
/// </summary>
public class EdmondsKarp : IDisposable
{
    private readonly LemonDigraph graph;
    private readonly ArcMap capacityMap;
    private bool disposed = false;

    #region P/Invoke declarations

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern long lemon_edmonds_karp(IntPtr graph, IntPtr capacity_map,
                                                  int source, int target,
                                                  out IntPtr flow_results, out int flow_count);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_free_results(IntPtr results);

    #endregion

    /// <summary>
    /// Creates a new Edmonds-Karp algorithm instance.
    /// </summary>
    /// <param name="graph">The digraph to run the algorithm on.</param>
    /// <param name="capacityMap">The arc capacity map.</param>
    public EdmondsKarp(LemonDigraph graph, ArcMap capacityMap)
    {
        this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
        this.capacityMap = capacityMap ?? throw new ArgumentNullException(nameof(capacityMap));

        if (capacityMap.ParentGraph != graph)
        {
            throw new ArgumentException("Capacity map must belong to the same graph", nameof(capacityMap));
        }
    }

    /// <summary>
    /// Creates a new Edmonds-Karp algorithm instance with a new capacity map.
    /// </summary>
    /// <param name="graph">The digraph to run the algorithm on.</param>
    public EdmondsKarp(LemonDigraph graph)
        : this(graph, new ArcMap(graph))
    {
    }

    /// <summary>
    /// Sets the capacity of an arc.
    /// </summary>
    /// <param name="arc">The arc to set capacity for.</param>
    /// <param name="capacity">The capacity value.</param>
    /// <returns>This instance for method chaining.</returns>
    public EdmondsKarp SetCapacity(Arc arc, long capacity)
    {
        ThrowIfDisposed();

        if (capacity < 0)
        {
            throw new ArgumentException("Capacity must be non-negative", nameof(capacity));
        }

        capacityMap[arc] = capacity;
        return this;
    }

    /// <summary>
    /// Gets the capacity map used by this algorithm.
    /// </summary>
    public ArcMap CapacityMap 
    { 
        get 
        { 
            ThrowIfDisposed();
            return capacityMap; 
        } 
    }

    /// <summary>
    /// Runs the Edmonds-Karp algorithm to find maximum flow.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <param name="target">The target node.</param>
    /// <returns>The maximum flow result.</returns>
    public MaxFlowResult Run(Node source, Node target)
    {
        ThrowIfDisposed();

        if (!graph.IsValid(source))
        {
            throw new ArgumentException("Invalid source node", nameof(source));
        }

        if (!graph.IsValid(target))
        {
            throw new ArgumentException("Invalid target node", nameof(target));
        }

        if (source == target)
        {
            throw new ArgumentException("Source and target must be different nodes");
        }

        IntPtr flowResultsPtr = IntPtr.Zero;
        
        try
        {
            long maxFlowValue = lemon_edmonds_karp(
                graph.Handle,
                capacityMap.Handle,
                source.Id,
                target.Id,
                out flowResultsPtr,
                out int flowCount);

            // Validate the flow value
            MarshalHelper.ValidateFlowValue(maxFlowValue);

            // Marshal the flow results using the shared helper
            EdgeFlow[] edgeFlows = MarshalHelper.MarshalFlowResults(flowResultsPtr, flowCount);

            return new MaxFlowResult(maxFlowValue, edgeFlows);
        }
        finally
        {
            // Always free the native results
            if (flowResultsPtr != IntPtr.Zero)
            {
                lemon_free_results(flowResultsPtr);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(EdmondsKarp));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            // Note: EdmondsKarp doesn't maintain any native state between calls,
            // so there's nothing to clean up here. The IDisposable implementation
            // is added for API consistency with Preflow and to prevent use after
            // the graph or capacity map might have been disposed.
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}