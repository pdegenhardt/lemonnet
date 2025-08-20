using System;

namespace LemonNet;

/// <summary>
/// Represents the flow on a single edge.
/// This is a lightweight value type containing the source, target, and flow amount.
/// </summary>
public readonly struct EdgeFlow : IEquatable<EdgeFlow>
{
    public Node Source { get; }
    public Node Target { get; }
    public double Flow { get; }

    public EdgeFlow(Node source, Node target, double flow)
    {
        Source = source;
        Target = target;
        Flow = flow;
    }

    public bool Equals(EdgeFlow other) =>
        Source.Equals(other.Source) &&
        Target.Equals(other.Target) &&
        Flow.Equals(other.Flow);

    public override bool Equals(object obj) =>
        obj is EdgeFlow flow && Equals(flow);

    public override int GetHashCode() =>
        HashCode.Combine(Source, Target, Flow);

    public override string ToString() =>
        $"Edge({Source} -> {Target}): Flow = {Flow}";

    public static bool operator ==(EdgeFlow left, EdgeFlow right) =>
        left.Equals(right);

    public static bool operator !=(EdgeFlow left, EdgeFlow right) =>
        !left.Equals(right);
}