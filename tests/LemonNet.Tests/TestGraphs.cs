using System;
using System.Collections.Generic;
using LemonNet;

namespace LemonNet.Tests;

/// <summary>
/// Provides standard test graphs for max flow algorithm testing.
/// This ensures both EdmondsKarp and Preflow are tested against the same graphs.
/// </summary>
public static class TestGraphs
{
    public class GraphData : IDisposable
    {
        public LemonDigraph Graph { get; }
        public ArcMap CapacityMap { get; }
        public Node Source { get; set; }
        public Node Target { get; set; }
        public long ExpectedMaxFlow { get; }
        public string Description { get; }
        
        public GraphData(string description, long expectedMaxFlow)
        {
            Graph = new LemonDigraph();
            CapacityMap = new ArcMap(Graph);
            Description = description;
            ExpectedMaxFlow = expectedMaxFlow;
        }
        
        public void Dispose()
        {
            CapacityMap?.Dispose();
            Graph?.Dispose();
        }
    }
    
    /// <summary>
    /// Simple 4-node graph with known max flow of 29.
    /// Structure: s -> v1 -> t
    ///           s -> v2 -> t
    ///           with v1 -> v2 connection
    /// </summary>
    public static GraphData CreateSimpleGraph()
    {
        var data = new GraphData("Simple 4-node graph", 29);
        
        var s = data.Graph.AddNode();
        var v1 = data.Graph.AddNode();
        var v2 = data.Graph.AddNode();
        var t = data.Graph.AddNode();
        
        data.Source = s;
        data.Target = t;
        
        // Set capacities
        data.CapacityMap[data.Graph.AddArc(s, v1)] = 16;
        data.CapacityMap[data.Graph.AddArc(s, v2)] = 13;
        data.CapacityMap[data.Graph.AddArc(v1, t)] = 12;
        data.CapacityMap[data.Graph.AddArc(v2, t)] = 20;
        data.CapacityMap[data.Graph.AddArc(v1, v2)] = 10;
        
        return data;
    }
    
    /// <summary>
    /// Linear path graph where max flow equals the bottleneck capacity.
    /// </summary>
    public static GraphData CreateLinearPathGraph()
    {
        var data = new GraphData("Linear path with bottleneck", 10);
        
        var s = data.Graph.AddNode();
        var v1 = data.Graph.AddNode();
        var v2 = data.Graph.AddNode();
        var t = data.Graph.AddNode();
        
        data.Source = s;
        data.Target = t;
        
        // Linear path with bottleneck at v1->v2
        data.CapacityMap[data.Graph.AddArc(s, v1)] = 20;
        data.CapacityMap[data.Graph.AddArc(v1, v2)] = 10;  // Bottleneck
        data.CapacityMap[data.Graph.AddArc(v2, t)] = 30;
        
        return data;
    }
    
    /// <summary>
    /// Complex 6-node graph for testing.
    /// </summary>
    public static GraphData CreateComplexGraph()
    {
        var data = new GraphData("Complex 6-node graph", 23);
        
        var nodes = new Node[6];
        for (int i = 0; i < 6; i++)
        {
            nodes[i] = data.Graph.AddNode();
        }
        
        data.Source = nodes[0];
        data.Target = nodes[5];
        
        // Create arcs with capacities
        var arcs = new (int from, int to, long capacity)[]
        {
            (0, 1, 16), (0, 2, 13),
            (1, 2, 10), (1, 3, 12),
            (2, 1, 4),  (2, 4, 14),
            (3, 2, 9),  (3, 5, 20),
            (4, 3, 7),  (4, 5, 4)
        };
        
        foreach (var (from, to, capacity) in arcs)
        {
            var arc = data.Graph.AddArc(nodes[from], nodes[to]);
            data.CapacityMap[arc] = capacity;
        }
        
        return data;
    }
    
    /// <summary>
    /// Disconnected graph where source and target are not connected.
    /// </summary>
    public static GraphData CreateDisconnectedGraph()
    {
        var data = new GraphData("Disconnected graph", 0);
        
        var s = data.Graph.AddNode();
        var island1 = data.Graph.AddNode();
        var island2 = data.Graph.AddNode();
        var t = data.Graph.AddNode();
        
        data.Source = s;
        data.Target = t;
        
        // Create two disconnected components
        data.CapacityMap[data.Graph.AddArc(s, island1)] = 10;
        data.CapacityMap[data.Graph.AddArc(island2, t)] = 10;
        // No connection between island1 and island2
        
        return data;
    }
    
    /// <summary>
    /// Single edge graph for basic testing.
    /// </summary>
    public static GraphData CreateSingleEdgeGraph()
    {
        var data = new GraphData("Single edge graph", 15);
        
        var s = data.Graph.AddNode();
        var t = data.Graph.AddNode();
        
        data.Source = s;
        data.Target = t;
        
        data.CapacityMap[data.Graph.AddArc(s, t)] = 15;
        
        return data;
    }
    
    /// <summary>
    /// Diamond-shaped graph with multiple paths.
    /// </summary>
    public static GraphData CreateDiamondGraph()
    {
        var data = new GraphData("Diamond graph", 19);
        
        var s = data.Graph.AddNode();
        var a = data.Graph.AddNode();
        var b = data.Graph.AddNode();
        var t = data.Graph.AddNode();
        
        data.Source = s;
        data.Target = t;
        
        // Two paths from s to t through a and b
        data.CapacityMap[data.Graph.AddArc(s, a)] = 10;
        data.CapacityMap[data.Graph.AddArc(s, b)] = 10;
        data.CapacityMap[data.Graph.AddArc(a, t)] = 10;
        data.CapacityMap[data.Graph.AddArc(b, t)] = 9;
        data.CapacityMap[data.Graph.AddArc(a, b)] = 1;  // Cross edge
        
        return data;
    }
    
    /// <summary>
    /// Creates an invalid node for testing purposes.
    /// This creates a graph with many nodes and returns a node that would be out of range
    /// for typical test graphs.
    /// </summary>
    public static Node CreateInvalidNodeForTesting()
    {
        // Create a temporary graph with many nodes to get a high ID node
        using var tempGraph = new LemonDigraph();
        Node lastNode = default;
        for (int i = 0; i < 100; i++)
        {
            lastNode = tempGraph.AddNode();
        }
        // Return the last node which will have ID 99, 
        // which should be invalid for our test graphs that have only 4-6 nodes
        return lastNode;
    }
    
    /// <summary>
    /// Creates a large layered graph for performance testing.
    /// </summary>
    public static GraphData CreateLargeLayeredGraph(int nodeCount = 100, int seed = 42)
    {
        var data = new GraphData($"Large layered graph ({nodeCount} nodes)", -1); // Max flow will vary
        
        var nodes = new Node[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            nodes[i] = data.Graph.AddNode();
        }
        
        data.Source = nodes[0];
        data.Target = nodes[nodeCount - 1];
        
        var random = new Random(seed);
        
        // Create a layered graph structure
        for (int layer = 0; layer < nodeCount - 1; layer++)
        {
            // Add 2-5 arcs from each node to nodes in the next "layer"
            int arcCount = random.Next(2, 6);
            for (int j = 0; j < arcCount && layer + j + 1 < nodeCount; j++)
            {
                var arc = data.Graph.AddArc(nodes[layer], nodes[Math.Min(layer + j + 1, nodeCount - 1)]);
                data.CapacityMap[arc] = random.Next(1, 100);
            }
        }
        
        return data;
    }
}