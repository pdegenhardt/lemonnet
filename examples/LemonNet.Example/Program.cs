using LemonNet;

Console.WriteLine("LemonNet Example - Maximum Flow Calculation");
Console.WriteLine("===========================================");

// Create a simple diamond-shaped graph:
//     1 
//    / \
//   0   3
//    \ /
//     2
//
// With capacities:
// 0->1: 10
// 0->2: 10  
// 1->3: 10
// 2->3: 10

using (var graph = new LemonDigraph())
{
    // Add nodes
    var source = graph.AddNode();
    var upper = graph.AddNode();
    var lower = graph.AddNode();
    var sink = graph.AddNode();
    
    Console.WriteLine($"Created graph with {graph.NodeCount} nodes");
    
    // Add arcs
    var arc01 = graph.AddArc(source, upper);
    var arc02 = graph.AddArc(source, lower);
    var arc13 = graph.AddArc(upper, sink);
    var arc23 = graph.AddArc(lower, sink);
    
    Console.WriteLine($"Added {graph.ArcCount} arcs");
    
    // Test both algorithms
    Console.WriteLine("\n--- Edmonds-Karp Algorithm ---");
    RunEdmondsKarp(graph, source, sink, arc01, arc02, arc13, arc23);
    
    Console.WriteLine("\n--- Preflow Algorithm ---");
    RunPreflow(graph, source, sink, arc01, arc02, arc13, arc23);
}

Console.WriteLine("\nExample completed successfully!");

void RunEdmondsKarp(LemonDigraph graph, Node source, Node sink, Arc arc01, Arc arc02, Arc arc13, Arc arc23)
{
    using (var edmondsKarp = new EdmondsKarp(graph))
    {
        // Set capacities
        edmondsKarp.SetCapacity(arc01, 10);
        edmondsKarp.SetCapacity(arc02, 10);
        edmondsKarp.SetCapacity(arc13, 10);
        edmondsKarp.SetCapacity(arc23, 10);
        
        // Run the algorithm
        var result = edmondsKarp.Run(source, sink);
        
        Console.WriteLine($"Maximum flow: {result.MaxFlowValue}");
        Console.WriteLine("Flow on each arc:");
        foreach (var flow in result.EdgeFlows)
        {
            var source = graph.Source(flow.Arc);
            var target = graph.Target(flow.Arc);
            Console.WriteLine($"  {source} -> {target}: {flow.Flow}");
        }
    }
}

void RunPreflow(LemonDigraph graph, Node source, Node sink, Arc arc01, Arc arc02, Arc arc13, Arc arc23)
{
    using (var preflow = new Preflow(graph))
    {
        // Set capacities
        preflow.SetCapacity(arc01, 10);
        preflow.SetCapacity(arc02, 10);
        preflow.SetCapacity(arc13, 10);
        preflow.SetCapacity(arc23, 10);
        
        // Run the algorithm
        var result = preflow.Run(source, sink);
        
        Console.WriteLine($"Maximum flow: {result.MaxFlowValue}");
        Console.WriteLine("Flow on each arc:");
        foreach (var flow in result.EdgeFlows)
        {
            var source = graph.Source(flow.Arc);
            var target = graph.Target(flow.Arc);
            Console.WriteLine($"  {source} -> {target}: {flow.Flow}");
        }
    }
}