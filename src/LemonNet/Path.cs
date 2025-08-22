using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LemonNet;

/// <summary>
/// Represents a path in a digraph as a sequence of arcs.
/// </summary>
public class Path : IEnumerable<Arc>
{
    private readonly Arc[] arcs;
    private readonly LemonDigraph graph;

    /// <summary>
    /// Creates a new path from a sequence of arcs.
    /// </summary>
    /// <param name="graph">The graph containing the path.</param>
    /// <param name="arcs">The sequence of arcs forming the path.</param>
    public Path(LemonDigraph graph, IEnumerable<Arc> arcs)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        
        this.graph = graph;
        this.arcs = arcs?.ToArray() ?? Array.Empty<Arc>();
    }

    /// <summary>
    /// Creates an empty path.
    /// </summary>
    /// <param name="graph">The graph containing the path.</param>
    public Path(LemonDigraph graph) : this(graph, Array.Empty<Arc>())
    {
    }

    /// <summary>
    /// Gets the graph containing this path.
    /// </summary>
    public LemonDigraph Graph => graph;

    /// <summary>
    /// Gets the number of arcs in the path.
    /// </summary>
    public int Length => arcs.Length;

    /// <summary>
    /// Gets whether the path is empty.
    /// </summary>
    public bool IsEmpty => arcs.Length == 0;

    /// <summary>
    /// Gets the arc at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the arc to get.</param>
    /// <returns>The arc at the specified index.</returns>
    public Arc this[int index] => arcs[index];

    /// <summary>
    /// Gets the source node of the path (the source of the first arc).
    /// Returns an invalid node if the path is empty.
    /// </summary>
    public Node Source
    {
        get
        {
            if (IsEmpty)
                return Node.Invalid;
            
            return graph.Source(arcs[0]);
        }
    }

    /// <summary>
    /// Gets the target node of the path (the target of the last arc).
    /// Returns an invalid node if the path is empty.
    /// </summary>
    public Node Target
    {
        get
        {
            if (IsEmpty)
                return Node.Invalid;
            
            return graph.Target(arcs[arcs.Length - 1]);
        }
    }

    /// <summary>
    /// Gets all the nodes in the path, including source and target.
    /// </summary>
    /// <returns>An enumerable of nodes in the path.</returns>
    public IEnumerable<Node> GetNodes()
    {
        if (IsEmpty)
            yield break;

        yield return graph.Source(arcs[0]);
        
        foreach (var arc in arcs)
        {
            yield return graph.Target(arc);
        }
    }

    /// <summary>
    /// Calculates the total cost of the path using the provided arc map.
    /// </summary>
    /// <param name="costMap">The arc map containing costs.</param>
    /// <returns>The total cost of the path.</returns>
    public double GetTotalCost(ArcMapDouble costMap)
    {
        if (costMap == null)
            throw new ArgumentNullException(nameof(costMap));

        double totalCost = 0.0;
        foreach (var arc in arcs)
        {
            totalCost += costMap[arc];
        }
        return totalCost;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the arcs in the path.
    /// </summary>
    public IEnumerator<Arc> GetEnumerator()
    {
        return ((IEnumerable<Arc>)arcs).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns a string representation of the path.
    /// </summary>
    public override string ToString()
    {
        if (IsEmpty)
            return "Path: (empty)";

        var nodes = GetNodes().ToList();
        return $"Path: {string.Join(" -> ", nodes.Select(n => n.ToString()))}";
    }
}