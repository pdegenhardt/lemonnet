using System;
using System.Runtime.InteropServices;

namespace LemonNet;

/// <summary>
/// Represents a node in the LEMON digraph.
/// This is a lightweight value type containing just an ID.
/// </summary>
public struct Node : IEquatable<Node>
{
    internal readonly int Id;

    internal Node(int id) => Id = id;

    /// <summary>
    /// Represents an invalid node.
    /// </summary>
    public static readonly Node Invalid = new Node(-1);

    /// <summary>
    /// Checks if this node is valid.
    /// </summary>
    public bool IsValid => Id >= 0;

    public bool Equals(Node other) => Id == other.Id;
    public override bool Equals(object obj) => obj is Node node && Equals(node);
    public override int GetHashCode() => Id;
    public override string ToString() => $"Node({Id})";

    public static bool operator ==(Node left, Node right) => left.Equals(right);
    public static bool operator !=(Node left, Node right) => !left.Equals(right);
}

/// <summary>
/// Represents an arc (directed edge) in the LEMON digraph.
/// This is a lightweight value type containing just an ID.
/// </summary>
public struct Arc : IEquatable<Arc>
{
    internal readonly int Id;

    internal Arc(int id) => Id = id;

    /// <summary>
    /// Represents an invalid arc.
    /// </summary>
    public static readonly Arc Invalid = new Arc(-1);

    /// <summary>
    /// Checks if this arc is valid.
    /// </summary>
    public bool IsValid => Id >= 0;

    public bool Equals(Arc other) => Id == other.Id;
    public override bool Equals(object obj) => obj is Arc arc && Equals(arc);
    public override int GetHashCode() => Id;
    public override string ToString() => $"Arc({Id})";

    public static bool operator ==(Arc left, Arc right) => left.Equals(right);
    public static bool operator !=(Arc left, Arc right) => !left.Equals(right);
}

/// <summary>
/// Represents a directed graph using the LEMON library.
/// </summary>
public class LemonDigraph : IDisposable
{
    private IntPtr graphHandle;
    private bool disposed = false;
    private int nodeCount = 0;
    private int arcCount = 0;

    #region P/Invoke declarations

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr lemon_create_graph();

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern void lemon_destroy_graph(IntPtr graph);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lemon_add_node(IntPtr graph);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lemon_add_arc(IntPtr graph, int source, int target);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lemon_arc_source(IntPtr graph, int arc);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lemon_arc_target(IntPtr graph, int arc);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lemon_node_count(IntPtr graph);

    [DllImport("lemon_wrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int lemon_arc_count(IntPtr graph);

    #endregion

    /// <summary>
    /// Creates a new LEMON digraph.
    /// </summary>
    public LemonDigraph()
    {
        graphHandle = lemon_create_graph();
        if (graphHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create LEMON graph");
        }
    }

    /// <summary>
    /// Gets the handle to the underlying native graph.
    /// This is used internally by algorithm classes.
    /// </summary>
    internal IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return graphHandle;
        }
    }

    /// <summary>
    /// Gets the number of nodes in the graph.
    /// </summary>
    public int NodeCount
    {
        get
        {
            ThrowIfDisposed();
            return nodeCount;
        }
    }

    /// <summary>
    /// Gets the number of arcs in the graph.
    /// </summary>
    public int ArcCount
    {
        get
        {
            ThrowIfDisposed();
            return arcCount;
        }
    }

    /// <summary>
    /// Adds a new node to the graph.
    /// </summary>
    /// <returns>The newly created node.</returns>
    public Node AddNode()
    {
        ThrowIfDisposed();

        int nodeId = lemon_add_node(graphHandle);
        if (nodeId < 0)
        {
            throw new InvalidOperationException("Failed to add node to graph");
        }

        nodeCount++;
        return new Node(nodeId);
    }

    /// <summary>
    /// Adds a new arc to the graph.
    /// </summary>
    /// <param name="source">The source node of the arc.</param>
    /// <param name="target">The target node of the arc.</param>
    /// <returns>The newly created arc.</returns>
    public Arc AddArc(Node source, Node target)
    {
        ThrowIfDisposed();

        if (!IsValid(source))
        {
            throw new ArgumentException("Invalid source node", nameof(source));
        }

        if (!IsValid(target))
        {
            throw new ArgumentException("Invalid target node", nameof(target));
        }

        int arcId = lemon_add_arc(graphHandle, source.Id, target.Id);
        if (arcId < 0)
        {
            throw new InvalidOperationException("Failed to add arc to graph");
        }

        arcCount++;
        return new Arc(arcId);
    }

    /// <summary>
    /// Gets the source node of an arc.
    /// </summary>
    /// <param name="arc">The arc to query.</param>
    /// <returns>The source node of the arc.</returns>
    public Node Source(Arc arc)
    {
        ThrowIfDisposed();

        if (!IsValid(arc))
        {
            throw new ArgumentException("Invalid arc", nameof(arc));
        }

        int nodeId = lemon_arc_source(graphHandle, arc.Id);
        return new Node(nodeId);
    }

    /// <summary>
    /// Gets the target node of an arc.
    /// </summary>
    /// <param name="arc">The arc to query.</param>
    /// <returns>The target node of the arc.</returns>
    public Node Target(Arc arc)
    {
        ThrowIfDisposed();

        if (!IsValid(arc))
        {
            throw new ArgumentException("Invalid arc", nameof(arc));
        }

        int nodeId = lemon_arc_target(graphHandle, arc.Id);
        return new Node(nodeId);
    }

    /// <summary>
    /// Checks if a node is valid for this graph.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <returns>True if the node is valid, false otherwise.</returns>
    public bool IsValid(Node node)
    {
        return node.Id >= 0 && node.Id < nodeCount;
    }

    /// <summary>
    /// Checks if an arc is valid for this graph.
    /// </summary>
    /// <param name="arc">The arc to validate.</param>
    /// <returns>True if the arc is valid, false otherwise.</returns>
    public bool IsValid(Arc arc)
    {
        return arc.Id >= 0 && arc.Id < arcCount;
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(LemonDigraph));
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (graphHandle != IntPtr.Zero)
            {
                lemon_destroy_graph(graphHandle);
                graphHandle = IntPtr.Zero;
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LemonDigraph()
    {
        Dispose(false);
    }
}