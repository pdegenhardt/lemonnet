using System;

namespace LemonNet;

/// <summary>
/// Represents the flow on a single arc.
/// This is a lightweight value type containing the arc and its flow amount.
/// </summary>
public readonly struct EdgeFlow : IEquatable<EdgeFlow>
{
    public Arc Arc { get; }
    public long Flow { get; }

    public EdgeFlow(Arc arc, long flow)
    {
        Arc = arc;
        Flow = flow;
    }

    public bool Equals(EdgeFlow other) =>
        Arc.Equals(other.Arc) &&
        Flow.Equals(other.Flow);

    public override bool Equals(object obj) =>
        obj is EdgeFlow flow && Equals(flow);

    public override int GetHashCode() =>
        HashCode.Combine(Arc, Flow);

    public override string ToString() =>
        $"Arc {Arc}: Flow = {Flow}";

    public static bool operator ==(EdgeFlow left, EdgeFlow right) =>
        left.Equals(right);

    public static bool operator !=(EdgeFlow left, EdgeFlow right) =>
        !left.Equals(right);
}