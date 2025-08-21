using System;
using System.Collections.Generic;

namespace LemonNet;

/// <summary>
/// Represents the result of a maximum flow computation.
/// </summary>
public class MaxFlowResult
{
    public long MaxFlowValue { get; }
    public IReadOnlyList<EdgeFlow> EdgeFlows { get; }

    public MaxFlowResult(long maxFlowValue, EdgeFlow[] edgeFlows)
    {
        MaxFlowValue = maxFlowValue;
        EdgeFlows = edgeFlows ?? Array.Empty<EdgeFlow>();
    }

    public override string ToString()
    {
        return $"Max Flow: {MaxFlowValue}, Edges with flow: {EdgeFlows.Count}";
    }
}