using System;
using System.Runtime.InteropServices;

namespace LemonNet;

/// <summary>
/// Represents a map that associates values with arcs in a LEMON digraph.
/// Currently supports long values only.
/// </summary>
public class ArcMap : IDisposable
{
    private IntPtr mapHandle;
    private LemonDigraph parentGraph;
    private bool disposed = false;

    #region P/Invoke declarations

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr lemon_create_arc_map_long(IntPtr graph);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_destroy_arc_map(IntPtr map);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_set_arc_value_long(IntPtr map, int arc, long value);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern long lemon_get_arc_value_long(IntPtr map, int arc);

    #endregion

    /// <summary>
    /// Creates a new arc map for the specified graph.
    /// </summary>
    /// <param name="graph">The graph this arc map is associated with.</param>
    public ArcMap(LemonDigraph graph)
    {
        if (graph == null)
        {
            throw new ArgumentNullException(nameof(graph));
        }

        parentGraph = graph;
        mapHandle = lemon_create_arc_map_long(graph.Handle);
        
        if (mapHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create arc map");
        }
    }

    /// <summary>
    /// Gets the handle to the underlying native arc map.
    /// This is used internally by algorithm classes.
    /// </summary>
    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return mapHandle;
        }
    }

    /// <summary>
    /// Gets the parent graph this arc map belongs to.
    /// </summary>
    public LemonDigraph ParentGraph
    {
        get
        {
            ThrowIfDisposed();
            return parentGraph;
        }
    }

    /// <summary>
    /// Sets the value associated with an arc.
    /// </summary>
    /// <param name="arc">The arc to set the value for.</param>
    /// <param name="value">The value to associate with the arc.</param>
    public void SetValue(Arc arc, long value)
    {
        ThrowIfDisposed();

        if (!parentGraph.IsValid(arc))
        {
            throw new ArgumentException("Invalid arc for this graph", nameof(arc));
        }

        lemon_set_arc_value_long(mapHandle, arc.Id, value);
    }

    /// <summary>
    /// Gets the value associated with an arc.
    /// </summary>
    /// <param name="arc">The arc to get the value for.</param>
    /// <returns>The value associated with the arc.</returns>
    public long GetValue(Arc arc)
    {
        ThrowIfDisposed();

        if (!parentGraph.IsValid(arc))
        {
            throw new ArgumentException("Invalid arc for this graph", nameof(arc));
        }

        return lemon_get_arc_value_long(mapHandle, arc.Id);
    }

    /// <summary>
    /// Indexer for convenient access to arc values.
    /// </summary>
    /// <param name="arc">The arc to access.</param>
    /// <returns>The value associated with the arc.</returns>
    public long this[Arc arc]
    {
        get => GetValue(arc);
        set => SetValue(arc, value);
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(ArcMap));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (mapHandle != IntPtr.Zero)
            {
                lemon_destroy_arc_map(mapHandle);
                mapHandle = IntPtr.Zero;
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ArcMap()
    {
        Dispose(false);
    }
}