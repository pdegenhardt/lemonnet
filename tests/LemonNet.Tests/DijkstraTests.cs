using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LemonNet.Tests;

public class DijkstraTests
{
    private readonly ITestOutputHelper output;

    public DijkstraTests(ITestOutputHelper output)
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

        using var dijkstra = new Dijkstra(graph, lengthMap);

        // Act
        var result = dijkstra.Run(node0, node3);

        // Assert
        Assert.True(result.TargetReached);
        Assert.Equal(5.0, result.Distance);
        Assert.NotNull(result.Path);
        Assert.Equal(2, result.Path.Length);
        output.WriteLine($"Shortest path distance: {result.Distance}");
        output.WriteLine($"Path: {result.Path}");
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

        using var dijkstra = new Dijkstra(graph, lengthMap);

        // Act
        var result = dijkstra.Run(node0, node2);

        // Assert
        Assert.False(result.TargetReached);
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

        using var dijkstra = new Dijkstra(graph, lengthMap);

        // Act
        var result = dijkstra.Run(node0, node0);

        // Assert
        Assert.True(result.TargetReached);
        Assert.Equal(0.0, result.Distance);
        Assert.NotNull(result.Path);
        Assert.Equal(0, result.Path.Length);
    }

    [Fact]
    public void LargerGraph_FindsOptimalPath()
    {
        // Arrange: Create a grid-like graph
        using var graph = new LemonDigraph();
        
        // Create 3x3 grid of nodes
        var nodes = new Node[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                nodes[i, j] = graph.AddNode();
            }
        }

        using var lengthMap = new ArcMapDouble(graph);

        // Add horizontal arcs
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var arc = graph.AddArc(nodes[i, j], nodes[i, j + 1]);
                lengthMap[arc] = 1.0;
            }
        }

        // Add vertical arcs
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var arc = graph.AddArc(nodes[i, j], nodes[i + 1, j]);
                lengthMap[arc] = 1.0;
            }
        }

        // Add diagonal shortcut with higher cost
        var diagonal = graph.AddArc(nodes[0, 0], nodes[2, 2]);
        lengthMap[diagonal] = 3.5;

        using var dijkstra = new Dijkstra(graph, lengthMap);

        // Act
        var result = dijkstra.Run(nodes[0, 0], nodes[2, 2]);

        // Assert
        Assert.True(result.TargetReached);
        Assert.Equal(3.5, result.Distance); // The diagonal is shorter than going around (4.0)
        Assert.NotNull(result.Path);
        Assert.Equal(1, result.Path.Length); // Single arc path
        output.WriteLine($"Grid path distance: {result.Distance}");
    }

    [Fact]
    public void FindDistance_ReturnsCorrectValue()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 5.5;

        using var dijkstra = new Dijkstra(graph, lengthMap);

        // Act
        var distance = dijkstra.FindDistance(node0, node1);

        // Assert
        Assert.Equal(5.5, distance);
    }

    [Fact]
    public void FindPath_ReturnsCorrectPath()
    {
        // Arrange
        using var graph = new LemonDigraph();
        var node0 = graph.AddNode();
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();

        var arc01 = graph.AddArc(node0, node1);
        var arc12 = graph.AddArc(node1, node2);

        using var lengthMap = new ArcMapDouble(graph);
        lengthMap[arc01] = 1.0;
        lengthMap[arc12] = 2.0;

        using var dijkstra = new Dijkstra(graph, lengthMap);

        // Act
        var path = dijkstra.FindPath(node0, node2);

        // Assert
        Assert.NotNull(path);
        Assert.Equal(2, path.Length);
        Assert.Equal(node0, path.Source);
        Assert.Equal(node2, path.Target);
    }
}