using System;
using System.Runtime.InteropServices;

namespace LemonNet;

/// <summary>
/// Represents a map that associates double values with nodes in a LEMON digraph.
/// </summary>
public class NodeMapDouble : IDisposable
{
    private IntPtr mapHandle;
    private LemonDigraph parentGraph;
    private bool disposed = false;

    #region P/Invoke declarations

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr lemon_create_node_map_double(IntPtr graph);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_destroy_node_map(IntPtr map);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_set_node_value_double(IntPtr map, int node, double value);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern double lemon_get_node_value_double(IntPtr map, int node);

    #endregion

    /// <summary>
    /// Creates a new node map for double values for the specified graph.
    /// </summary>
    /// <param name="graph">The graph this node map is associated with.</param>
    public NodeMapDouble(LemonDigraph graph)
    {
        if (graph == null)
        {
            throw new ArgumentNullException(nameof(graph));
        }

        parentGraph = graph;
        mapHandle = lemon_create_node_map_double(graph.Handle);
        
        if (mapHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create node map");
        }
    }

    /// <summary>
    /// Gets the handle to the native node map.
    /// </summary>
    internal IntPtr Handle => mapHandle;

    /// <summary>
    /// Sets the value associated with a node.
    /// </summary>
    /// <param name="node">The node to set the value for.</param>
    /// <param name="value">The value to associate with the node.</param>
    public void SetValue(Node node, double value)
    {
        ThrowIfDisposed();
        
        if (!node.IsValid)
        {
            throw new ArgumentException("Invalid node", nameof(node));
        }

        lemon_set_node_value_double(mapHandle, node.Id, value);
    }

    /// <summary>
    /// Gets the value associated with a node.
    /// </summary>
    /// <param name="node">The node to get the value for.</param>
    /// <returns>The value associated with the node.</returns>
    public double GetValue(Node node)
    {
        ThrowIfDisposed();
        
        if (!node.IsValid)
        {
            throw new ArgumentException("Invalid node", nameof(node));
        }

        return lemon_get_node_value_double(mapHandle, node.Id);
    }

    /// <summary>
    /// Indexer for convenient access to node values.
    /// </summary>
    /// <param name="node">The node to access.</param>
    /// <returns>The value associated with the node.</returns>
    public double this[Node node]
    {
        get => GetValue(node);
        set => SetValue(node, value);
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
                lemon_destroy_node_map(mapHandle);
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

    ~NodeMapDouble()
    {
        Dispose(disposing: false);
    }
}