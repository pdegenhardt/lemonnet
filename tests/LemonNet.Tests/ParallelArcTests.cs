using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using LemonNet;

namespace LemonNet.Tests;

/// <summary>
/// Tests specifically focused on parallel arc support in LEMON graphs.
/// Parallel arcs are multiple arcs between the same pair of nodes.
/// </summary>
public class ParallelArcTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private LemonDigraph graph;
    private ArcMap capacityMap;

    public ParallelArcTests(ITestOutputHelper output)
    {
        this.output = output;
        this.graph = new LemonDigraph();
        this.capacityMap = new ArcMap(graph);
    }

    public void Dispose()
    {
        capacityMap?.Dispose();
        graph?.Dispose();
    }

    [Fact]
    public void AddArc_SameNodePair_CreatesParallelArcs()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();

        // Act
        var arc1 = graph.AddArc(source, target);
        var arc2 = graph.AddArc(source, target);
        var arc3 = graph.AddArc(source, target);

        // Assert
        Assert.NotEqual(arc1, arc2);
        Assert.NotEqual(arc2, arc3);
        Assert.NotEqual(arc1, arc3);
        Assert.Equal(3, graph.ArcCount);
        
        output.WriteLine($"Created 3 parallel arcs: {arc1}, {arc2}, {arc3}");
    }

    [Fact]
    public void ParallelArcs_HaveIndependentCapacities()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();

        // Act
        var arc1 = graph.AddArc(source, target);
        var arc2 = graph.AddArc(source, target);
        var arc3 = graph.AddArc(source, target);

        capacityMap[arc1] = 10;
        capacityMap[arc2] = 20;
        capacityMap[arc3] = 30;

        // Assert
        Assert.Equal(10, capacityMap[arc1]);
        Assert.Equal(20, capacityMap[arc2]);
        Assert.Equal(30, capacityMap[arc3]);
    }

    [Fact]
    public void MaxFlow_ParallelArcs_SumsCapacities()
    {
        // Arrange
        var source = graph.AddNode();
        var sink = graph.AddNode();

        // Create 3 parallel arcs with different capacities
        capacityMap[graph.AddArc(source, sink)] = 10;
        capacityMap[graph.AddArc(source, sink)] = 5;
        capacityMap[graph.AddArc(source, sink)] = 15;

        var edmonds = new EdmondsKarp(graph, capacityMap);

        // Act
        var result = edmonds.Run(source, sink);
        
        // Debug output
        output.WriteLine($"EdmondsKarp max flow value: {result.MaxFlowValue}");
        output.WriteLine($"Expected max flow: 30");
        output.WriteLine($"Total edge flows: {result.EdgeFlows.Count}");
        foreach (var flow in result.EdgeFlows)
        {
            var s = graph.Source(flow.Arc);
            var target = graph.Target(flow.Arc);
            output.WriteLine($"  Arc {flow.Arc} ({s} -> {target}): flow = {flow.Flow}");
        }

        // Assert
        Assert.Equal(30, result.MaxFlowValue);  // Sum of all capacities
        Assert.Equal(3, result.EdgeFlows.Count);   // All 3 arcs should have flow
        
        var totalFlow = result.EdgeFlows.Sum(e => e.Flow);
        output.WriteLine($"Total flow sum: {totalFlow}");
        
        // Debug: Check for overflow or corrupted values
        foreach (var flow in result.EdgeFlows)
        {
            if (flow.Flow < 0 || flow.Flow > 1000000)
            {
                var s = graph.Source(flow.Arc);
                var target = graph.Target(flow.Arc);
                output.WriteLine($"WARNING: Suspicious flow value {flow.Flow} on arc {flow.Arc} ({s} -> {target})");
            }
        }
        
        Assert.Equal(30L, totalFlow);
        
        output.WriteLine($"Total max flow through parallel arcs: {result.MaxFlowValue}");
        foreach (var flow in result.EdgeFlows)
        {
            var s = graph.Source(flow.Arc);
            var target = graph.Target(flow.Arc);
            output.WriteLine($"  Arc {flow.Arc} from {s} to {target}: {flow.Flow}");
        }
    }

    [Fact]
    public void MaxFlow_MixedParallelArcs_HandlesComplexNetwork()
    {
        // Arrange - Network with both parallel and non-parallel arcs
        var source = graph.AddNode();
        var middle = graph.AddNode();
        var sink = graph.AddNode();

        // Two parallel arcs from source to middle
        capacityMap[graph.AddArc(source, middle)] = 10;
        capacityMap[graph.AddArc(source, middle)] = 15;

        // Three parallel arcs from middle to sink
        capacityMap[graph.AddArc(middle, sink)] = 8;
        capacityMap[graph.AddArc(middle, sink)] = 7;
        capacityMap[graph.AddArc(middle, sink)] = 5;

        var edmonds = new EdmondsKarp(graph, capacityMap);

        // Act
        var result = edmonds.Run(source, sink);

        // Assert
        // Max flow limited by smaller total capacity (middle->sink = 20)
        Assert.Equal(20, result.MaxFlowValue);
        
        output.WriteLine($"Max flow in mixed parallel network: {result.MaxFlowValue}");
        output.WriteLine($"Source->Middle capacity: 25, Middle->Sink capacity: 20");
        output.WriteLine($"Bottleneck at Middle->Sink limits flow to 20");
    }

    [Fact]
    public void ParallelArcs_WithZeroCapacity_HandledCorrectly()
    {
        // Arrange
        var source = graph.AddNode();
        var sink = graph.AddNode();

        capacityMap[graph.AddArc(source, sink)] = 10;
        capacityMap[graph.AddArc(source, sink)] = 0;   // Zero capacity
        capacityMap[graph.AddArc(source, sink)] = 5;

        var edmonds = new EdmondsKarp(graph, capacityMap);

        // Act
        var result = edmonds.Run(source, sink);

        // Assert
        Assert.Equal(15, result.MaxFlowValue);  // 10 + 0 + 5
        
        // The arc with zero capacity might not appear in results
        var nonZeroFlows = result.EdgeFlows.Where(e => e.Flow > 0).Count();
        output.WriteLine($"Arcs with non-zero flow: {nonZeroFlows}");
    }

    [Fact]
    public void ParallelArcs_BidirectionalFlow_WorksCorrectly()
    {
        // Arrange - Parallel arcs in both directions
        var nodeA = graph.AddNode();
        var nodeB = graph.AddNode();

        // Two arcs from A to B
        var arcAB1 = graph.AddArc(nodeA, nodeB);
        var arcAB2 = graph.AddArc(nodeA, nodeB);
        
        // Two arcs from B to A (opposite direction)
        var arcBA1 = graph.AddArc(nodeB, nodeA);
        var arcBA2 = graph.AddArc(nodeB, nodeA);

        // Assert - All arcs are distinct
        Assert.Equal(4, graph.ArcCount);
        Assert.NotEqual(arcAB1, arcAB2);
        Assert.NotEqual(arcBA1, arcBA2);
        
        // Verify arc directions
        Assert.Equal(nodeA, graph.Source(arcAB1));
        Assert.Equal(nodeB, graph.Target(arcAB1));
        Assert.Equal(nodeB, graph.Source(arcBA1));
        Assert.Equal(nodeA, graph.Target(arcBA1));
        
        output.WriteLine("Created bidirectional parallel arcs:");
        output.WriteLine($"  A->B: {arcAB1}, {arcAB2}");
        output.WriteLine($"  B->A: {arcBA1}, {arcBA2}");
    }

    [Fact]
    public void ParallelArcs_LargeScale_PerformanceTest()
    {
        // Arrange - Many parallel arcs
        var source = graph.AddNode();
        var sink = graph.AddNode();
        const int parallelArcCount = 100;
        
        for (int i = 0; i < parallelArcCount; i++)
        {
            capacityMap[graph.AddArc(source, sink)] = i + 1;
        }

        var edmonds = new EdmondsKarp(graph, capacityMap);

        // Act
        var startTime = DateTime.UtcNow;
        var result = edmonds.Run(source, sink);
        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

        // Assert
        // Sum of 1+2+3+...+100 = 100*101/2 = 5050
        Assert.Equal(5050, result.MaxFlowValue);
        Assert.Equal(parallelArcCount, result.EdgeFlows.Count);
        
        output.WriteLine($"Processed {parallelArcCount} parallel arcs in {elapsedMs:F2}ms");
        output.WriteLine($"Total flow: {result.MaxFlowValue}");
    }

    [Fact]
    public void Source_Target_ParallelArcs_ReturnCorrectNodes()
    {
        // Arrange
        var nodeA = graph.AddNode();
        var nodeB = graph.AddNode();
        
        var arc1 = graph.AddArc(nodeA, nodeB);
        var arc2 = graph.AddArc(nodeA, nodeB);
        var arc3 = graph.AddArc(nodeA, nodeB);

        // Act & Assert - All parallel arcs have same source and target
        Assert.Equal(nodeA, graph.Source(arc1));
        Assert.Equal(nodeA, graph.Source(arc2));
        Assert.Equal(nodeA, graph.Source(arc3));
        
        Assert.Equal(nodeB, graph.Target(arc1));
        Assert.Equal(nodeB, graph.Target(arc2));
        Assert.Equal(nodeB, graph.Target(arc3));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void ParallelArcs_VariousCounts_WorkCorrectly(int arcCount)
    {
        // Arrange
        var source = graph.AddNode();
        var sink = graph.AddNode();
        
        for (int i = 0; i < arcCount; i++)
        {
            capacityMap[graph.AddArc(source, sink)] = 10;
        }

        var edmonds = new EdmondsKarp(graph, capacityMap);

        // Act
        var result = edmonds.Run(source, sink);

        // Assert
        Assert.Equal(arcCount * 10, result.MaxFlowValue);
        Assert.Equal(arcCount, result.EdgeFlows.Count);
        
        output.WriteLine($"With {arcCount} parallel arcs of capacity 10 each:");
        output.WriteLine($"  Expected flow: {arcCount * 10}");
        output.WriteLine($"  Actual flow: {result.MaxFlowValue}");
    }
}