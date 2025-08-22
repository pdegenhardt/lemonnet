using System;
using System.Runtime.InteropServices;

namespace LemonNet;

/// <summary>
/// Represents a map that associates double values with arcs in a LEMON digraph.
/// </summary>
public class ArcMapDouble : IDisposable
{
    private IntPtr mapHandle;
    private LemonDigraph parentGraph;
    private bool disposed = false;

    #region P/Invoke declarations

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr lemon_create_arc_map_double(IntPtr graph);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_destroy_arc_map(IntPtr map);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_set_arc_value_double(IntPtr map, int arc, double value);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern double lemon_get_arc_value_double(IntPtr map, int arc);

    #endregion

    /// <summary>
    /// Creates a new arc map for double values for the specified graph.
    /// </summary>
    /// <param name="graph">The graph this arc map is associated with.</param>
    public ArcMapDouble(LemonDigraph graph)
    {
        if (graph == null)
        {
            throw new ArgumentNullException(nameof(graph));
        }

        parentGraph = graph;
        mapHandle = lemon_create_arc_map_double(graph.Handle);
        
        if (mapHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create arc map");
        }
    }

    /// <summary>
    /// Gets the handle to the native arc map.
    /// </summary>
    internal IntPtr Handle => mapHandle;

    /// <summary>
    /// Sets the value associated with an arc.
    /// </summary>
    /// <param name="arc">The arc to set the value for.</param>
    /// <param name="value">The value to associate with the arc.</param>
    public void SetValue(Arc arc, double value)
    {
        ThrowIfDisposed();
        
        if (!arc.IsValid)
        {
            throw new ArgumentException("Invalid arc", nameof(arc));
        }

        lemon_set_arc_value_double(mapHandle, arc.Id, value);
    }

    /// <summary>
    /// Gets the value associated with an arc.
    /// </summary>
    /// <param name="arc">The arc to get the value for.</param>
    /// <returns>The value associated with the arc.</returns>
    public double GetValue(Arc arc)
    {
        ThrowIfDisposed();
        
        if (!arc.IsValid)
        {
            throw new ArgumentException("Invalid arc", nameof(arc));
        }

        return lemon_get_arc_value_double(mapHandle, arc.Id);
    }

    /// <summary>
    /// Indexer for convenient access to arc values.
    /// </summary>
    /// <param name="arc">The arc to access.</param>
    /// <returns>The value associated with the arc.</returns>
    public double this[Arc arc]
    {
        get => GetValue(arc);
        set => SetValue(arc, value);
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
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~ArcMapDouble()
    {
        Dispose(disposing: false);
    }
}