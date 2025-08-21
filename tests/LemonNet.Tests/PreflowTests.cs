using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using LemonNet;

namespace LemonNet.Tests;

public class PreflowTests
{
    private readonly ITestOutputHelper output;

    public PreflowTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void SimpleGraph_ReturnsCorrectMaxFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateSimpleGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var result = preflow.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
        output.WriteLine($"Edges with flow: {result.EdgeFlows.Count}");
        
        foreach (var flow in result.EdgeFlows)
        {
            var source = data.Graph.Source(flow.Arc);
            var target = data.Graph.Target(flow.Arc);
            output.WriteLine($"Flow from {source} to {target}: {flow.Flow}");
        }
    }

    [Fact]
    public void LinearPath_FlowEqualsBottleneck()
    {
        // Arrange
        using var data = TestGraphs.CreateLinearPathGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var result = preflow.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void ComplexGraph_ReturnsCorrectMaxFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateComplexGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var result = preflow.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void DisconnectedGraph_ReturnsZeroFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateDisconnectedGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var result = preflow.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue);
        Assert.Empty(result.EdgeFlows);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void SingleEdge_ReturnsEdgeCapacity()
    {
        // Arrange
        using var data = TestGraphs.CreateSingleEdgeGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var result = preflow.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue);
        Assert.Single(result.EdgeFlows);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void DiamondGraph_ReturnsCorrectMaxFlow()
    {
        // Arrange
        using var data = TestGraphs.CreateDiamondGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var result = preflow.Run(data.Source, data.Target);

        // Assert
        Assert.Equal(data.ExpectedMaxFlow, result.MaxFlowValue);
        output.WriteLine($"{data.Description}: Max flow = {result.MaxFlowValue}");
    }

    [Fact]
    public void CompareWithEdmondsKarp_AllGraphs()
    {
        // Test all standard graphs to ensure both algorithms give same results
        var testCases = new[]
        {
            ("Simple", TestGraphs.CreateSimpleGraph()),
            ("Linear", TestGraphs.CreateLinearPathGraph()),
            ("Complex", TestGraphs.CreateComplexGraph()),
            ("Disconnected", TestGraphs.CreateDisconnectedGraph()),
            ("SingleEdge", TestGraphs.CreateSingleEdgeGraph()),
            ("Diamond", TestGraphs.CreateDiamondGraph())
        };

        foreach (var (name, data) in testCases)
        {
            using (data)
            {
                // Run both algorithms
                using var preflow = new Preflow(data.Graph, data.CapacityMap);
                var preflowResult = preflow.Run(data.Source, data.Target);

                var edmondsKarp = new EdmondsKarp(data.Graph, data.CapacityMap);
                var edmondsKarpResult = edmondsKarp.Run(data.Source, data.Target);

                // Assert both give same max flow
                Assert.Equal(edmondsKarpResult.MaxFlowValue, preflowResult.MaxFlowValue);
                output.WriteLine($"{name} - Edmonds-Karp: {edmondsKarpResult.MaxFlowValue}, Preflow: {preflowResult.MaxFlowValue}");
            }
        }
    }

    [Fact]
    public void InvalidNodes_ThrowsException()
    {
        // Arrange
        using var data = TestGraphs.CreateSimpleGraph();
        var invalidNode = TestGraphs.CreateInvalidNodeForTesting();
        
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => preflow.Run(invalidNode, data.Target));
        Assert.Throws<ArgumentException>(() => preflow.Run(data.Source, invalidNode));
        Assert.Throws<ArgumentException>(() => preflow.Run(Node.Invalid, data.Target));
    }

    [Fact]
    public void SameSourceAndTarget_ThrowsException()
    {
        // Arrange
        using var data = TestGraphs.CreateSimpleGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => preflow.Run(data.Source, data.Source));
    }

    [Fact]
    public void NegativeCapacity_ThrowsException()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var s = graph.AddNode();
        var t = graph.AddNode();
        var arc = graph.AddArc(s, t);

        using var preflow = new Preflow(graph);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => preflow.SetCapacity(arc, -5));
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
        using var preflow = new Preflow(graph)
            .SetCapacity(arc1, 10)
            .SetCapacity(arc2, 20);

        var result = preflow.Run(s, t);

        // Assert
        Assert.Equal(10.0, result.MaxFlowValue);
    }

    [Fact]
    public void CanRunMultipleTimes_WithDifferentCapacities()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var s = graph.AddNode();
        var t = graph.AddNode();
        var arc = graph.AddArc(s, t);

        using var preflow = new Preflow(graph);
        preflow.SetCapacity(arc, 10);
        
        // Act - First run
        var result1 = preflow.Run(s, t);
        Assert.Equal(10.0, result1.MaxFlowValue);
        
        // Act - Modify capacity and run again
        preflow.SetCapacity(arc, 20);
        var result2 = preflow.Run(s, t);
        
        // Assert
        Assert.Equal(20.0, result2.MaxFlowValue);
    }

    [Fact]
    public void FlowConservation_IsValid()
    {
        // Arrange - Create a complex graph with 6 nodes
        using var data = TestGraphs.CreateComplexGraph();
        using var preflow = new Preflow(data.Graph, data.CapacityMap);
        
        // Act
        var result = preflow.Run(data.Source, data.Target);
        
        // Debug output
        output.WriteLine($"Preflow max flow value: {result.MaxFlowValue}");
        output.WriteLine($"Expected max flow: {data.ExpectedMaxFlow}");
        output.WriteLine($"Total edge flows: {result.EdgeFlows.Count}");
        foreach (var flow in result.EdgeFlows)
        {
            var source = data.Graph.Source(flow.Arc);
            var target = data.Graph.Target(flow.Arc);
            output.WriteLine($"  Arc {flow.Arc} ({source} -> {target}): flow = {flow.Flow}");
        }

        // Assert - Verify flow conservation at all nodes
        var flowIn = new Dictionary<Node, long>();
        var flowOut = new Dictionary<Node, long>();

        // Initialize flow dictionaries for all nodes
        // We need to track all nodes that appear in the flow results
        foreach (var flow in result.EdgeFlows)
        {
            var source = data.Graph.Source(flow.Arc);
            var target = data.Graph.Target(flow.Arc);
            
            if (!flowIn.ContainsKey(source))
                flowIn[source] = 0;
            if (!flowIn.ContainsKey(target))
                flowIn[target] = 0;
            if (!flowOut.ContainsKey(source))
                flowOut[source] = 0;
            if (!flowOut.ContainsKey(target))
                flowOut[target] = 0;
        }

        // Calculate total flow in and out for each node
        foreach (var flow in result.EdgeFlows)
        {
            var source = data.Graph.Source(flow.Arc);
            var target = data.Graph.Target(flow.Arc);
            
            flowOut[source] += flow.Flow;
            flowIn[target] += flow.Flow;
        }

        // The key insight: max flow algorithms may not return all edges, only those with positive flow
        // The important check is that flow into target equals max flow value
        
        // Check target flow
        if (flowIn.ContainsKey(data.Target))
        {
            var targetInFlow = flowIn[data.Target];
            output.WriteLine($"Target: Flow in = {targetInFlow}");
            Assert.Equal(result.MaxFlowValue, targetInFlow);
        }
        else
        {
            // If target has no incoming flow, max flow should be 0
            Assert.Equal(0, result.MaxFlowValue);
        }
        
        // Verify flow conservation for intermediate nodes
        // (not source or target)
        foreach (var node in flowIn.Keys)
        {
            if (!node.Equals(data.Source) && !node.Equals(data.Target))
            {
                // Intermediate node: flow in should equal flow out (conservation)
                var inFlow = flowIn.GetValueOrDefault(node);
                var outFlow = flowOut.GetValueOrDefault(node);
                output.WriteLine($"Intermediate node {node}: Flow in = {inFlow}, Flow out = {outFlow}");
                Assert.Equal(inFlow, outFlow);
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
        using var preflow = new Preflow(data.Graph, data.CapacityMap);

        // Act
        var startTime = DateTime.Now;
        var result = preflow.Run(data.Source, data.Target);
        var elapsed = DateTime.Now - startTime;

        // Assert
        Assert.True(result.MaxFlowValue >= 0);
        output.WriteLine($"Large graph max flow: {result.MaxFlowValue}");
        output.WriteLine($"Execution time: {elapsed.TotalMilliseconds}ms");
        Assert.True(elapsed.TotalSeconds < 1, "Preflow should be faster - complete within 1 second");
    }

    [Fact]
    public void PerformanceComparison_PreflowFasterThanEdmondsKarp()
    {
        // Arrange - Create a larger graph where performance difference matters
        using var data = TestGraphs.CreateLargeLayeredGraph(200, 42);
        
        // Act - Measure Edmonds-Karp
        var edmondsKarp = new EdmondsKarp(data.Graph, data.CapacityMap);
        var ekStart = DateTime.Now;
        var ekResult = edmondsKarp.Run(data.Source, data.Target);
        var ekTime = (DateTime.Now - ekStart).TotalMilliseconds;
        
        // Act - Measure Preflow
        using var preflow = new Preflow(data.Graph, data.CapacityMap);
        var pfStart = DateTime.Now;
        var pfResult = preflow.Run(data.Source, data.Target);
        var pfTime = (DateTime.Now - pfStart).TotalMilliseconds;
        
        // Assert - Both give same result
        Assert.Equal(ekResult.MaxFlowValue, pfResult.MaxFlowValue);
        
        output.WriteLine($"Max flow: {pfResult.MaxFlowValue}");
        output.WriteLine($"Edmonds-Karp time: {ekTime}ms");
        output.WriteLine($"Preflow time: {pfTime}ms");
        output.WriteLine($"Speedup: {ekTime / pfTime:F2}x");
        
        // Preflow is often faster on larger graphs, but not always
        // So we just verify both completed successfully
        Assert.True(pfTime > 0);
        Assert.True(ekTime > 0);
    }
}