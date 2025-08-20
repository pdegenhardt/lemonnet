using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using LemonNet;

namespace LemonNet.Tests;

public class EdmondsKarpTests
{
    private readonly ITestOutputHelper output;

    public EdmondsKarpTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void SimpleGraph_ReturnsCorrectMaxFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateSimpleGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue, 6);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
        output.WriteLine($"Edges with flow: {result.EdgeFlows.Count}");
    }

    [Fact]
    public void LinearPath_FlowEqualsBottleneck()
    {
        // Arrange
        using var data = TestGraphs.CreateLinearPathGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue, 6);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void ComplexGraph_ReturnsCorrectMaxFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateComplexGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue, 6);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void DisconnectedGraph_ReturnsZeroFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateDisconnectedGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue, 6);
        Assert.Empty(result.EdgeFlows);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void SingleEdge_ReturnsEdgeCapacity()
    {
        // Arrange
        using var data = TestGraphs.CreateSingleEdgeGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue, 6);
        Assert.Single(result.EdgeFlows);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void DiamondGraph_ReturnsCorrectMaxFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateDiamondGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue, 6);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void InvalidNodes_ThrowsException()
    {
        // Arrange
        using var data = TestGraphs.CreateSimpleGraph();
        var invalidNode = TestGraphs.CreateInvalidNodeForTesting();
        
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => edmonds.Run(invalidNode, data.Target));
        Assert.Throws<ArgumentException>(() => edmonds.Run(data.Source, invalidNode));
        Assert.Throws<ArgumentException>(() => edmonds.Run(Node.Invalid, data.Target));
    }

    [Fact]
    public void SameSourceAndTarget_ThrowsException()
    {
        // Arrange
        using var data = TestGraphs.CreateSimpleGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => edmonds.Run(data.Source, data.Source));
    }

    [Fact]
    public void NegativeCapacity_ThrowsException()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var s = graph.AddNode();
        var t = graph.AddNode();
        var arc = graph.AddArc(s, t);

        var edmonds = new EdmondsKarp(graph);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => edmonds.SetCapacity(arc, -5));
    }

    [Fact]
    public void MethodChaining_WorksCorrectly()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var s = graph.AddNode();
        var v = graph.AddNode();
        var t = graph.AddNode();

        var arc1 = graph.AddArc(s, v);
        var arc2 = graph.AddArc(v, t);

        // Act - Test method chaining
        var edmonds = new EdmondsKarp(graph)
            .SetCapacity(arc1, 10)
            .SetCapacity(arc2, 20);

        var result = edmonds.Run(s, t);

        // Assert
        Assert.Equal(10.0, result.MaxFlowValue);
    }

    [Fact]
    public void FlowConservation_IsValid()
    {
        // Arrange - Create a complex graph with 6 nodes
        using var data = TestGraphs.CreateComplexGraph();
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var result = edmonds.Run(data.Source, data.Target);

        // Assert - Verify flow conservation at all nodes
        var flowIn = new Dictionary<Node, double>();
        var flowOut = new Dictionary<Node, double>();

        // Initialize flow dictionaries for all nodes
        // We need to track all nodes that appear in the flow results
        foreach (var flow in result.EdgeFlows)
        {
            if (!flowIn.ContainsKey(flow.Source))
                flowIn[flow.Source] = 0;
            if (!flowIn.ContainsKey(flow.Target))
                flowIn[flow.Target] = 0;
            if (!flowOut.ContainsKey(flow.Source))
                flowOut[flow.Source] = 0;
            if (!flowOut.ContainsKey(flow.Target))
                flowOut[flow.Target] = 0;
        }

        // Calculate total flow in and out for each node
        foreach (var flow in result.EdgeFlows)
        {
            flowOut[flow.Source] += flow.Flow;
            flowIn[flow.Target] += flow.Flow;
        }

        // Verify flow conservation for intermediate nodes
        // (not source or target)
        foreach (var node in flowIn.Keys)
        {
            if (node.Equals(data.Source))
            {
                // Source: flow out should equal max flow
                output.WriteLine($"Source: Flow out = {flowOut.GetValueOrDefault(node):F2}");
                Assert.Equal(result.MaxFlowValue, flowOut.GetValueOrDefault(node), 6);
            }
            else if (node.Equals(data.Target))
            {
                // Target: flow in should equal max flow
                output.WriteLine($"Target: Flow in = {flowIn.GetValueOrDefault(node):F2}");
                Assert.Equal(result.MaxFlowValue, flowIn.GetValueOrDefault(node), 6);
            }
            else
            {
                // Intermediate node: flow in should equal flow out (conservation)
                var inFlow = flowIn.GetValueOrDefault(node);
                var outFlow = flowOut.GetValueOrDefault(node);
                output.WriteLine($"Intermediate node: Flow in = {inFlow:F2}, Flow out = {outFlow:F2}");
                Assert.Equal(inFlow, outFlow, 6);
            }
        }

        Assert.True(result.MaxFlowValue > 0);
        output.WriteLine($"Flow conservation verified with max flow: {result.MaxFlowValue}");
    }

    [Fact]
    public void LargeGraph_Performance()
    {
        // Arrange
        using var data = TestGraphs.CreateLargeLayeredGraph(100, 42);
        var edmonds = new EdmondsKarp(data.Graph, data.CapacityMap);

        // Act
        var startTime = DateTime.Now;
        var result = edmonds.Run(data.Source, data.Target);
        var elapsed = DateTime.Now - startTime;

        // Assert
        Assert.True(result.MaxFlowValue >= 0);
        output.WriteLine($"Large graph max flow: {result.MaxFlowValue}");
        output.WriteLine($"Execution time: {elapsed.TotalMilliseconds}ms");
        Assert.True(elapsed.TotalSeconds < 2, "Algorithm should complete within 2 seconds");
    }
}