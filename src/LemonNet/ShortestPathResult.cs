using System;

namespace LemonNet;

/// <summary>
/// Represents the result of a shortest path computation.
/// </summary>
public class ShortestPathResult
{
    /// <summary>
    /// Gets the distance from source to target.
    /// Returns double.PositiveInfinity if the target is not reachable.
    /// </summary>
    public double Distance { get; }

    /// <summary>
    /// Gets the path from source to target.
    /// Returns null if no path exists.
    /// </summary>
    public Path? Path { get; }

    /// <summary>
    /// Gets whether the target was reached from the source.
    /// </summary>
    public bool TargetReached { get; }

    /// <summary>
    /// Gets whether a negative cycle was detected (Bellman-Ford only).
    /// </summary>
    public bool HasNegativeCycle { get; }

    /// <summary>
    /// Creates a new shortest path result.
    /// </summary>
    /// <param name="distance">The distance from source to target.</param>
    /// <param name="path">The path from source to target.</param>
    /// <param name="targetReached">Whether the target was reached.</param>
    /// <param name="hasNegativeCycle">Whether a negative cycle was detected.</param>
    public ShortestPathResult(double distance, Path? path, bool targetReached, bool hasNegativeCycle = false)
    {
        Distance = distance;
        Path = path;
        TargetReached = targetReached;
        HasNegativeCycle = hasNegativeCycle;
    }

    /// <summary>
    /// Creates a result for an unreachable target.
    /// </summary>
    public static ShortestPathResult Unreachable()
    {
        return new ShortestPathResult(double.PositiveInfinity, null, false, false);
    }

    /// <summary>
    /// Creates a result for a negative cycle detection.
    /// </summary>
    public static ShortestPathResult NegativeCycle()
    {
        return new ShortestPathResult(double.PositiveInfinity, null, false, true);
    }

    public override string ToString()
    {
        if (HasNegativeCycle)
            return "Shortest Path: Negative cycle detected";
        
        if (!TargetReached)
            return "Shortest Path: Target unreachable";
        
        return $"Shortest Path: Distance = {Distance}, Path length = {Path?.Length ?? 0}";
    }
}