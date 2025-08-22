using BenchmarkDotNet.Attributes;
using LemonNet;

[MemoryDiagnoser]
public class MaxFlowBenchmarks
{
    private LemonDigraph? smallGraph;
    private Node smallSourceNode;
    private Node smallSinkNode;
    private List<Arc> smallArcs = new();
    private List<int> smallCapacities = new();
    
    private LemonDigraph? largeGraph;
    private Node largeSourceNode;
    private Node largeSinkNode;
    private List<Arc> largeArcs = new();
    private List<int> largeCapacities = new();
    
    private LemonDigraph? extraLargeGraph;
    private Node extraLargeSourceNode;
    private Node extraLargeSinkNode;
    private List<Arc> extraLargeArcs = new();
    private List<int> extraLargeCapacities = new();
    
    [GlobalSetup]
    public void Setup()
    {
        SetupSmallGraph();
        SetupLargeGraph();
        SetupExtraLargeGraph();
    }
    
    private void SetupSmallGraph()
    {
        smallGraph = new LemonDigraph();
        smallArcs.Clear();
        smallCapacities.Clear();
        
        // Create a layered flow network with approximately 1000 arcs
        // Using 5 layers with varying number of nodes per layer
        var layers = new List<List<Node>>();
        var random = new Random(42); // Fixed seed for reproducibility
        
        // Layer 0: Source
        var sourceLayer = new List<Node> { smallGraph.AddNode() };
        smallSourceNode = sourceLayer[0];
        layers.Add(sourceLayer);
        
        // Layer 1: 20 nodes
        var layer1 = new List<Node>();
        for (int i = 0; i < 20; i++)
        {
            layer1.Add(smallGraph.AddNode());
        }
        layers.Add(layer1);
        
        // Layer 2: 30 nodes
        var layer2 = new List<Node>();
        for (int i = 0; i < 30; i++)
        {
            layer2.Add(smallGraph.AddNode());
        }
        layers.Add(layer2);
        
        // Layer 3: 20 nodes
        var layer3 = new List<Node>();
        for (int i = 0; i < 20; i++)
        {
            layer3.Add(smallGraph.AddNode());
        }
        layers.Add(layer3);
        
        // Layer 4: Sink
        var sinkLayer = new List<Node> { smallGraph.AddNode() };
        smallSinkNode = sinkLayer[0];
        layers.Add(sinkLayer);
        
        // Connect layers with random arcs
        // Source to Layer 1: connect to all nodes
        foreach (var node in layer1)
        {
            var arc = smallGraph.AddArc(smallSourceNode, node);
            smallArcs.Add(arc);
            smallCapacities.Add(random.Next(10, 101)); // Capacity between 10-100
        }
        
        // Layer 1 to Layer 2: random connections (aim for ~400 arcs)
        foreach (var fromNode in layer1)
        {
            // Each node connects to 15-25 nodes in the next layer
            var connectionCount = random.Next(15, 26);
            var targetNodes = layer2.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = smallGraph.AddArc(fromNode, toNode);
                smallArcs.Add(arc);
                smallCapacities.Add(random.Next(5, 51)); // Capacity between 5-50
            }
        }
        
        // Layer 2 to Layer 3: random connections (aim for ~400 arcs)
        foreach (var fromNode in layer2)
        {
            // Each node connects to 10-20 nodes in the next layer
            var connectionCount = random.Next(10, 21);
            var targetNodes = layer3.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = smallGraph.AddArc(fromNode, toNode);
                smallArcs.Add(arc);
                smallCapacities.Add(random.Next(5, 51)); // Capacity between 5-50
            }
        }
        
        // Layer 3 to Sink: connect all nodes to sink
        foreach (var node in layer3)
        {
            var arc = smallGraph.AddArc(node, smallSinkNode);
            smallArcs.Add(arc);
            smallCapacities.Add(random.Next(10, 101)); // Capacity between 10-100
        }
        
        Console.WriteLine($"Created small graph with {smallGraph.NodeCount} nodes and {smallGraph.ArcCount} arcs");
    }
    
    private void SetupLargeGraph()
    {
        largeGraph = new LemonDigraph();
        largeArcs.Clear();
        largeCapacities.Clear();
        
        // Create a layered flow network with approximately 100,000 arcs
        // Using 6 layers with more nodes per layer
        var layers = new List<List<Node>>();
        var random = new Random(42); // Fixed seed for reproducibility
        
        // Layer 0: Source
        var sourceLayer = new List<Node> { largeGraph.AddNode() };
        largeSourceNode = sourceLayer[0];
        layers.Add(sourceLayer);
        
        // Layer 1: 100 nodes
        var layer1 = new List<Node>();
        for (int i = 0; i < 100; i++)
        {
            layer1.Add(largeGraph.AddNode());
        }
        layers.Add(layer1);
        
        // Layer 2: 200 nodes
        var layer2 = new List<Node>();
        for (int i = 0; i < 200; i++)
        {
            layer2.Add(largeGraph.AddNode());
        }
        layers.Add(layer2);
        
        // Layer 3: 300 nodes
        var layer3 = new List<Node>();
        for (int i = 0; i < 300; i++)
        {
            layer3.Add(largeGraph.AddNode());
        }
        layers.Add(layer3);
        
        // Layer 4: 200 nodes
        var layer4 = new List<Node>();
        for (int i = 0; i < 200; i++)
        {
            layer4.Add(largeGraph.AddNode());
        }
        layers.Add(layer4);
        
        // Layer 5: 100 nodes
        var layer5 = new List<Node>();
        for (int i = 0; i < 100; i++)
        {
            layer5.Add(largeGraph.AddNode());
        }
        layers.Add(layer5);
        
        // Layer 6: Sink
        var sinkLayer = new List<Node> { largeGraph.AddNode() };
        largeSinkNode = sinkLayer[0];
        layers.Add(sinkLayer);
        
        // Connect layers with random arcs
        // Source to Layer 1: connect to all nodes
        foreach (var node in layer1)
        {
            var arc = largeGraph.AddArc(largeSourceNode, node);
            largeArcs.Add(arc);
            largeCapacities.Add(random.Next(100, 1001)); // Capacity between 100-1000
        }
        
        // Layer 1 to Layer 2: each node connects to ~40 nodes
        foreach (var fromNode in layer1)
        {
            var connectionCount = random.Next(35, 46);
            var targetNodes = layer2.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = largeGraph.AddArc(fromNode, toNode);
                largeArcs.Add(arc);
                largeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 2 to Layer 3: each node connects to ~60 nodes
        foreach (var fromNode in layer2)
        {
            var connectionCount = random.Next(55, 66);
            var targetNodes = layer3.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = largeGraph.AddArc(fromNode, toNode);
                largeArcs.Add(arc);
                largeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 3 to Layer 4: each node connects to ~40 nodes
        foreach (var fromNode in layer3)
        {
            var connectionCount = random.Next(35, 46);
            var targetNodes = layer4.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = largeGraph.AddArc(fromNode, toNode);
                largeArcs.Add(arc);
                largeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 4 to Layer 5: each node connects to ~25 nodes
        foreach (var fromNode in layer4)
        {
            var connectionCount = random.Next(20, 31);
            var targetNodes = layer5.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = largeGraph.AddArc(fromNode, toNode);
                largeArcs.Add(arc);
                largeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 5 to Sink: connect all nodes to sink
        foreach (var node in layer5)
        {
            var arc = largeGraph.AddArc(node, largeSinkNode);
            largeArcs.Add(arc);
            largeCapacities.Add(random.Next(100, 1001)); // Capacity between 100-1000
        }
        
        Console.WriteLine($"Created large graph with {largeGraph.NodeCount} nodes and {largeGraph.ArcCount} arcs");
    }
    
    private void SetupExtraLargeGraph()
    {
        extraLargeGraph = new LemonDigraph();
        extraLargeArcs.Clear();
        extraLargeCapacities.Clear();
        
        // Create a layered flow network with approximately 500,000 arcs
        // Using 7 layers with many nodes per layer
        var layers = new List<List<Node>>();
        var random = new Random(42); // Fixed seed for reproducibility
        
        // Layer 0: Source
        var sourceLayer = new List<Node> { extraLargeGraph.AddNode() };
        extraLargeSourceNode = sourceLayer[0];
        layers.Add(sourceLayer);
        
        // Layer 1: 200 nodes
        var layer1 = new List<Node>();
        for (int i = 0; i < 200; i++)
        {
            layer1.Add(extraLargeGraph.AddNode());
        }
        layers.Add(layer1);
        
        // Layer 2: 400 nodes
        var layer2 = new List<Node>();
        for (int i = 0; i < 400; i++)
        {
            layer2.Add(extraLargeGraph.AddNode());
        }
        layers.Add(layer2);
        
        // Layer 3: 600 nodes
        var layer3 = new List<Node>();
        for (int i = 0; i < 600; i++)
        {
            layer3.Add(extraLargeGraph.AddNode());
        }
        layers.Add(layer3);
        
        // Layer 4: 600 nodes
        var layer4 = new List<Node>();
        for (int i = 0; i < 600; i++)
        {
            layer4.Add(extraLargeGraph.AddNode());
        }
        layers.Add(layer4);
        
        // Layer 5: 400 nodes
        var layer5 = new List<Node>();
        for (int i = 0; i < 400; i++)
        {
            layer5.Add(extraLargeGraph.AddNode());
        }
        layers.Add(layer5);
        
        // Layer 6: 200 nodes
        var layer6 = new List<Node>();
        for (int i = 0; i < 200; i++)
        {
            layer6.Add(extraLargeGraph.AddNode());
        }
        layers.Add(layer6);
        
        // Layer 7: Sink
        var sinkLayer = new List<Node> { extraLargeGraph.AddNode() };
        extraLargeSinkNode = sinkLayer[0];
        layers.Add(sinkLayer);
        
        // Connect layers with random arcs
        // Source to Layer 1: connect to all nodes
        foreach (var node in layer1)
        {
            var arc = extraLargeGraph.AddArc(extraLargeSourceNode, node);
            extraLargeArcs.Add(arc);
            extraLargeCapacities.Add(random.Next(100, 1001)); // Capacity between 100-1000
        }
        
        // Layer 1 to Layer 2: each node connects to ~80 nodes
        foreach (var fromNode in layer1)
        {
            var connectionCount = random.Next(75, 86);
            var targetNodes = layer2.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = extraLargeGraph.AddArc(fromNode, toNode);
                extraLargeArcs.Add(arc);
                extraLargeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 2 to Layer 3: each node connects to ~90 nodes
        foreach (var fromNode in layer2)
        {
            var connectionCount = random.Next(85, 96);
            var targetNodes = layer3.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = extraLargeGraph.AddArc(fromNode, toNode);
                extraLargeArcs.Add(arc);
                extraLargeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 3 to Layer 4: each node connects to ~80 nodes
        foreach (var fromNode in layer3)
        {
            var connectionCount = random.Next(75, 86);
            var targetNodes = layer4.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = extraLargeGraph.AddArc(fromNode, toNode);
                extraLargeArcs.Add(arc);
                extraLargeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 4 to Layer 5: each node connects to ~60 nodes
        foreach (var fromNode in layer4)
        {
            var connectionCount = random.Next(55, 66);
            var targetNodes = layer5.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = extraLargeGraph.AddArc(fromNode, toNode);
                extraLargeArcs.Add(arc);
                extraLargeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 5 to Layer 6: each node connects to ~50 nodes
        foreach (var fromNode in layer5)
        {
            var connectionCount = random.Next(45, 56);
            var targetNodes = layer6.OrderBy(x => random.Next()).Take(connectionCount).ToList();
            
            foreach (var toNode in targetNodes)
            {
                var arc = extraLargeGraph.AddArc(fromNode, toNode);
                extraLargeArcs.Add(arc);
                extraLargeCapacities.Add(random.Next(50, 501)); // Capacity between 50-500
            }
        }
        
        // Layer 6 to Sink: connect all nodes to sink
        foreach (var node in layer6)
        {
            var arc = extraLargeGraph.AddArc(node, extraLargeSinkNode);
            extraLargeArcs.Add(arc);
            extraLargeCapacities.Add(random.Next(100, 1001)); // Capacity between 100-1000
        }
        
        Console.WriteLine($"Created extra large graph with {extraLargeGraph.NodeCount} nodes and {extraLargeGraph.ArcCount} arcs");
    }
    
    [GlobalCleanup]
    public void Cleanup()
    {
        smallGraph?.Dispose();
        largeGraph?.Dispose();
        extraLargeGraph?.Dispose();
    }
    
    [Benchmark]
    public MaxFlowResult BenchmarkEdmondsKarp()
    {
        using var edmondsKarp = new EdmondsKarp(smallGraph!);
        
        // Set capacities
        for (int i = 0; i < smallArcs.Count; i++)
        {
            edmondsKarp.SetCapacity(smallArcs[i], smallCapacities[i]);
        }
        
        // Run the algorithm
        return edmondsKarp.Run(smallSourceNode, smallSinkNode);
    }
    
    [Benchmark]
    public MaxFlowResult BenchmarkPreflow()
    {
        using var preflow = new Preflow(smallGraph!);
        
        // Set capacities
        for (int i = 0; i < smallArcs.Count; i++)
        {
            preflow.SetCapacity(smallArcs[i], smallCapacities[i]);
        }
        
        // Run the algorithm
        return preflow.Run(smallSourceNode, smallSinkNode);
    }
    
    [Benchmark]
    public MaxFlowResult BenchmarkEdmondsKarpLarge()
    {
        using var edmondsKarp = new EdmondsKarp(largeGraph!);
        
        // Set capacities
        for (int i = 0; i < largeArcs.Count; i++)
        {
            edmondsKarp.SetCapacity(largeArcs[i], largeCapacities[i]);
        }
        
        // Run the algorithm
        return edmondsKarp.Run(largeSourceNode, largeSinkNode);
    }
    
    [Benchmark]
    public MaxFlowResult BenchmarkPreflowLarge()
    {
        using var preflow = new Preflow(largeGraph!);
        
        // Set capacities
        for (int i = 0; i < largeArcs.Count; i++)
        {
            preflow.SetCapacity(largeArcs[i], largeCapacities[i]);
        }
        
        // Run the algorithm
        return preflow.Run(largeSourceNode, largeSinkNode);
    }
    
    [Benchmark]
    public MaxFlowResult BenchmarkEdmondsKarpExtraLarge()
    {
        using var edmondsKarp = new EdmondsKarp(extraLargeGraph!);
        
        // Set capacities
        for (int i = 0; i < extraLargeArcs.Count; i++)
        {
            edmondsKarp.SetCapacity(extraLargeArcs[i], extraLargeCapacities[i]);
        }
        
        // Run the algorithm
        return edmondsKarp.Run(extraLargeSourceNode, extraLargeSinkNode);
    }
    
    [Benchmark]
    public MaxFlowResult BenchmarkPreflowExtraLarge()
    {
        using var preflow = new Preflow(extraLargeGraph!);
        
        // Set capacities
        for (int i = 0; i < extraLargeArcs.Count; i++)
        {
            preflow.SetCapacity(extraLargeArcs[i], extraLargeCapacities[i]);
        }
        
        // Run the algorithm
        return preflow.Run(extraLargeSourceNode, extraLargeSinkNode);
    }
}