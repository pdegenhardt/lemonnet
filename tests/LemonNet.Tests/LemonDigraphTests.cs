using System;
using Xunit;
using Xunit.Abstractions;
using LemonNet;

namespace LemonNet.Tests;

public class LemonDigraphTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private LemonDigraph graph;

    public LemonDigraphTests(ITestOutputHelper output)
    {
        this.output = output;
        this.graph = new LemonDigraph();
    }

    public void Dispose()
    {
        graph?.Dispose();
    }

    [Fact]
    public void AddNode_ReturnsValidNode()
    {
        // Act
        var node = graph.AddNode();

        // Assert
        Assert.True(node.IsValid);
        Assert.Equal(1, graph.NodeCount);
    }

    [Fact]
    public void AddNode_Multiple_ReturnsUniqueNodes()
    {
        // Act
        var node1 = graph.AddNode();
        var node2 = graph.AddNode();
        var node3 = graph.AddNode();

        // Assert
        Assert.NotEqual(node1, node2);
        Assert.NotEqual(node2, node3);
        Assert.NotEqual(node1, node3);
        Assert.Equal(3, graph.NodeCount);
    }

    [Fact]
    public void AddArc_ValidNodes_ReturnsValidArc()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();

        // Act
        var arc = graph.AddArc(source, target);

        // Assert
        Assert.True(arc.IsValid);
        Assert.Equal(1, graph.ArcCount);
    }

    [Fact]
    public void AddArc_InvalidSource_ThrowsException()
    {
        // Arrange
        var validNode = graph.AddNode();
        var invalidNode = Node.Invalid;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.AddArc(invalidNode, validNode));
    }

    [Fact]
    public void AddArc_InvalidTarget_ThrowsException()
    {
        // Arrange
        var validNode = graph.AddNode();
        var invalidNode = Node.Invalid;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.AddArc(validNode, invalidNode));
    }

    [Fact]
    public void Source_ReturnsCorrectNode()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);

        // Act
        var retrievedSource = graph.Source(arc);

        // Assert
        Assert.Equal(source, retrievedSource);
    }

    [Fact]
    public void Target_ReturnsCorrectNode()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);

        // Act
        var retrievedTarget = graph.Target(arc);

        // Assert
        Assert.Equal(target, retrievedTarget);
    }

    [Fact]
    public void IsValid_Node_ReturnsTrueForValidNode()
    {
        // Arrange
        var node = graph.AddNode();

        // Act & Assert
        Assert.True(graph.IsValid(node));
    }

    [Fact]
    public void IsValid_Node_ReturnsFalseForInvalidNode()
    {
        // Arrange
        var invalidNode = Node.Invalid;
        // Node with ID that doesn't exist in graph
        var outOfRangeNode = default(Node);

        // Act & Assert
        Assert.False(graph.IsValid(invalidNode));
        Assert.False(graph.IsValid(outOfRangeNode));
    }

    [Fact]
    public void IsValid_Arc_ReturnsTrueForValidArc()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);

        // Act & Assert
        Assert.True(graph.IsValid(arc));
    }

    [Fact]
    public void IsValid_Arc_ReturnsFalseForInvalidArc()
    {
        // Arrange
        var invalidArc = Arc.Invalid;
        // Arc with ID that doesn't exist in graph
        var outOfRangeArc = default(Arc);

        // Act & Assert
        Assert.False(graph.IsValid(invalidArc));
        Assert.False(graph.IsValid(outOfRangeArc));
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Arrange
        var tempGraph = new LemonDigraph();
        tempGraph.AddNode();

        // Act & Assert - Should not throw
        tempGraph.Dispose();
        tempGraph.Dispose();
    }

    [Fact]
    public void Operations_AfterDispose_ThrowObjectDisposedException()
    {
        // Arrange
        var tempGraph = new LemonDigraph();
        tempGraph.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => tempGraph.AddNode());
        Assert.Throws<ObjectDisposedException>(() => tempGraph.NodeCount);
        Assert.Throws<ObjectDisposedException>(() => tempGraph.ArcCount);
    }

    [Fact]
    public void NodeStruct_EqualityOperators_WorkCorrectly()
    {
        // Arrange
        var tempGraph = new LemonDigraph();
        var node1 = tempGraph.AddNode();
        var node2 = node1; // Same node
        var node3 = tempGraph.AddNode();

        // Assert
        Assert.True(node1 == node2);
        Assert.False(node1 == node3);
        Assert.False(node1 != node2);
        Assert.True(node1 != node3);
        Assert.Equal(node1.GetHashCode(), node2.GetHashCode());
        tempGraph.Dispose();
    }

    [Fact]
    public void ArcStruct_EqualityOperators_WorkCorrectly()
    {
        // Arrange
        var tempGraph = new LemonDigraph();
        var n1 = tempGraph.AddNode();
        var n2 = tempGraph.AddNode();
        var arc1 = tempGraph.AddArc(n1, n2);
        var arc2 = arc1; // Same arc
        var arc3 = tempGraph.AddArc(n1, n2);

        // Assert
        Assert.True(arc1 == arc2);
        Assert.False(arc1 == arc3);
        Assert.False(arc1 != arc2);
        Assert.True(arc1 != arc3);
        Assert.Equal(arc1.GetHashCode(), arc2.GetHashCode());
        tempGraph.Dispose();
    }
}