using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LemonNet.Tests;

public class BellmanFordTests
{
    private readonly ITestOutputHelper output;

    public BellmanFordTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void SimpleGraph_FindsShortestPath()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();
        var node3 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);
        var arc02 = graph.AddArc(node0, node2);
        var arc13 = graph.AddArc(node1, node3);
        var arc23 = graph.AddArc(node2, node3);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 2.0;
        lengthMap[arc02] = 4.0;
        lengthMap[arc13] = 3.0;
        lengthMap[arc23] = 1.0;

        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var result = bellmanFord.Run(node0, node3);

        // Assert
        Assert.True(result.TargetReached);
        Assert.False(result.HasNegativeCycle);
        Assert.Equal(5.0, result.Distance);
        Assert.NotNull(result.Path);
        Assert.Equal(2, result.Path.Length);
        output.WriteLine($"Shortest path distance: {result.Distance}");
        output.WriteLine($"Path: {result.Path}");
    }

    [Fact]
    public void NegativeWeights_FindsCorrectPath()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);
        var arc12 = graph.AddArc(node1, node2);
        var arc02 = graph.AddArc(node0, node2);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 1.0;
        lengthMap[arc12] = -3.0;  // Negative weight
        lengthMap[arc02] = 2.0;

        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var result = bellmanFord.Run(node0, node2);

        // Assert
        Assert.True(result.TargetReached);
        Assert.False(result.HasNegativeCycle);
        Assert.Equal(-2.0, result.Distance); // Path through node1 is shorter due to negative weight
        Assert.NotNull(result.Path);
        Assert.Equal(2, result.Path.Length);
        output.WriteLine($"Path with negative weight distance: {result.Distance}");
    }

    [Fact]
    public void NegativeCycle_DetectsCorrectly()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);
        var arc12 = graph.AddArc(node1, node2);
        var arc20 = graph.AddArc(node2, node0);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 1.0;
        lengthMap[arc12] = -2.0;
        lengthMap[arc20] = -1.0;  // Creates a negative cycle

        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var result = bellmanFord.Run(node0, node2);

        // Assert
        Assert.True(result.HasNegativeCycle);
        Assert.False(result.TargetReached);
        Assert.Equal(double.PositiveInfinity, result.Distance);
        Assert.Null(result.Path);
        output.WriteLine("Negative cycle detected as expected");
    }

    [Fact]
    public void UnreachableTarget_ReturnsInfinity()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 1.0;

        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var result = bellmanFord.Run(node0, node2);

        // Assert
        Assert.False(result.TargetReached);
        Assert.False(result.HasNegativeCycle);
        Assert.Equal(double.PositiveInfinity, result.Distance);
        Assert.Null(result.Path);
    }

    [Fact]
    public void EmptyPath_SourceEqualsTarget()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 1.0;

        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var result = bellmanFord.Run(node0, node0);

        // Assert
        Assert.True(result.TargetReached);
        Assert.False(result.HasNegativeCycle);
        Assert.Equal(0.0, result.Distance);
        Assert.NotNull(result.Path);
        Assert.Equal(0, result.Path.Length);
    }

    [Fact]
    public void HasNegativeCycle_MethodWorks()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);
        var arc12 = graph.AddArc(node1, node2);
        var arc21 = graph.AddArc(node2, node1); // Create cycle

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 1.0;
        lengthMap[arc12] = -1.0;
        lengthMap[arc21] = -2.0;  // Negative cycle between node1 and node2

        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var hasNegativeCycle = bellmanFord.HasNegativeCycle(node0);

        // Assert
        Assert.True(hasNegativeCycle);
        output.WriteLine("Negative cycle detection method works correctly");
    }

    [Fact]
    public void CompareWithDijkstra_SameResultForPositiveWeights()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();
        var node3 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);
        var arc02 = graph.AddArc(node0, node2);
        var arc13 = graph.AddArc(node1, node3);
        var arc23 = graph.AddArc(node2, node3);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 2.0;
        lengthMap[arc02] = 4.0;
        lengthMap[arc13] = 3.0;
        lengthMap[arc23] = 1.0;

        using var dijkstra = new Dijkstra(graph, lengthMap);
        using var bellmanFord = new BellmanFord(graph, lengthMap);

        // Act
        var dijkstraResult = dijkstra.Run(node0, node3);
        var bellmanFordResult = bellmanFord.Run(node0, node3);

        // Assert
        Assert.Equal(dijkstraResult.Distance, bellmanFordResult.Distance);
        Assert.Equal(dijkstraResult.TargetReached, bellmanFordResult.TargetReached);
        output.WriteLine($"Both algorithms found distance: {dijkstraResult.Distance}");
    }
}