using System;
using Xunit;
using Xunit.Abstractions;
using LemonNet;

namespace LemonNet.Tests;

public class ArcMapTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private LemonDigraph graph;
    private ArcMap arcMap;

    public ArcMapTests(ITestOutputHelper output)
    {
        this.output = output;
        this.graph = new LemonDigraph();
        this.arcMap = new ArcMap(graph);
    }

    public void Dispose()
    {
        arcMap?.Dispose();
        graph?.Dispose();
    }

    [Fact]
    public void Constructor_ValidGraph_CreatesMap()
    {
        // Act - Constructor called in setup
        
        // Assert
        Assert.NotNull(arcMap);
        Assert.Equal(graph, arcMap.ParentGraph);
    }

    [Fact]
    public void Constructor_NullGraph_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ArcMap(null!));
    }

    [Fact]
    public void SetValue_ValidArc_StoresValue()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);
        const double value = 42.5;

        // Act
        arcMap.SetValue(arc, value);

        // Assert
        Assert.Equal(value, arcMap.GetValue(arc));
    }

    [Fact]
    public void GetValue_UnsetArc_ReturnsZero()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);

        // Act
        var value = arcMap.GetValue(arc);

        // Assert
        Assert.Equal(0.0, value);
    }

    [Fact]
    public void Indexer_Set_StoresValue()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);
        const double value = 33.3;

        // Act
        arcMap[arc] = value;

        // Assert
        Assert.Equal(value, arcMap[arc]);
    }

    [Fact]
    public void Indexer_Get_RetrievesValue()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);
        const double value = 55.5;
        arcMap.SetValue(arc, value);

        // Act
        var retrievedValue = arcMap[arc];

        // Assert
        Assert.Equal(value, retrievedValue);
    }

    [Fact]
    public void SetValue_InvalidArc_ThrowsException()
    {
        // Arrange
        var invalidArc = Arc.Invalid;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => arcMap.SetValue(invalidArc, 10.0));
    }

    [Fact]
    public void GetValue_InvalidArc_ThrowsException()
    {
        // Arrange
        var invalidArc = Arc.Invalid;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => arcMap.GetValue(invalidArc));
    }

    [Fact]
    public void SetValue_ArcFromDifferentGraph_ThrowsException()
    {
        // Arrange
        using var otherGraph = new LemonDigraph();
        var n1 = otherGraph.AddNode();
        var n2 = otherGraph.AddNode();
        var arcFromOtherGraph = otherGraph.AddArc(n1, n2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => arcMap.SetValue(arcFromOtherGraph, 10.0));
    }

    [Fact]
    public void MultipleArcs_StoreIndependentValues()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc1 = graph.AddArc(source, target);
        var arc2 = graph.AddArc(source, target);
        var arc3 = graph.AddArc(source, target);

        // Act
        arcMap[arc1] = 10.0;
        arcMap[arc2] = 20.0;
        arcMap[arc3] = 30.0;

        // Assert
        Assert.Equal(10.0, arcMap[arc1]);
        Assert.Equal(20.0, arcMap[arc2]);
        Assert.Equal(30.0, arcMap[arc3]);
        
        output.WriteLine("Multiple arcs between same nodes have independent values:");
        output.WriteLine($"Arc1: {arcMap[arc1]}");
        output.WriteLine($"Arc2: {arcMap[arc2]}");
        output.WriteLine($"Arc3: {arcMap[arc3]}");
    }

    [Fact]
    public void SetValue_OverwriteExisting_UpdatesValue()
    {
        // Arrange
        var source = graph.AddNode();
        var target = graph.AddNode();
        var arc = graph.AddArc(source, target);

        // Act
        arcMap[arc] = 10.0;
        arcMap[arc] = 20.0;
        arcMap[arc] = 30.0;

        // Assert
        Assert.Equal(30.0, arcMap[arc]);
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Arrange
        using var tempGraph = new LemonDigraph();
        var tempMap = new ArcMap(tempGraph);

        // Act & Assert - Should not throw
        tempMap.Dispose();
        tempMap.Dispose();
    }

    [Fact]
    public void Operations_AfterDispose_ThrowObjectDisposedException()
    {
        // Arrange
        using var tempGraph = new LemonDigraph();
        var tempMap = new ArcMap(tempGraph);
        var n1 = tempGraph.AddNode();
        var n2 = tempGraph.AddNode();
        var arc = tempGraph.AddArc(n1, n2);
        tempMap.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => tempMap.SetValue(arc, 10.0));
        Assert.Throws<ObjectDisposedException>(() => tempMap.GetValue(arc));
        Assert.Throws<ObjectDisposedException>(() => tempMap[arc] = 10.0);
        Assert.Throws<ObjectDisposedException>(() => _ = tempMap[arc]);
        Assert.Throws<ObjectDisposedException>(() => _ = tempMap.ParentGraph);
    }

    [Fact]
    public void ParentGraph_ReturnsCorrectGraph()
    {
        // Assert
        Assert.Same(graph, arcMap.ParentGraph);
    }
}