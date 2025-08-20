using System;
using System.Runtime.InteropServices;

namespace LemonNet.Internal;

/// <summary>
/// Internal helper class for marshaling flow results from native code.
/// </summary>
internal static class MarshalHelper
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeFlowResult
    {
        public int source;
        public int target;
        public double flow;
    }

    /// <summary>
    /// Marshals native flow results to managed EdgeFlow array.
    /// </summary>
    /// <param name="flowResultsPtr">Pointer to native flow results.</param>
    /// <param name="flowCount">Number of flow results.</param>
    /// <returns>Array of EdgeFlow objects.</returns>
    /// <exception cref="ArgumentException">If flowCount is invalid.</exception>
    internal static EdgeFlow[] MarshalFlowResults(IntPtr flowResultsPtr, int flowCount)
    {
        // Validate flow count to prevent potential memory corruption
        if (flowCount < 0)
        {
            throw new ArgumentException("Flow count cannot be negative", nameof(flowCount));
        }

        // Prevent unreasonably large allocations that could indicate corruption
        const int maxReasonableFlowCount = 1_000_000; // 1 million edges should be more than enough
        if (flowCount > maxReasonableFlowCount)
        {
            throw new ArgumentException($"Flow count {flowCount} exceeds reasonable limit", nameof(flowCount));
        }

        if (flowResultsPtr == IntPtr.Zero || flowCount == 0)
        {
            return Array.Empty<EdgeFlow>();
        }

        var edgeFlows = new EdgeFlow[flowCount];

        unsafe
        {
            var results = new ReadOnlySpan<NativeFlowResult>((void*)flowResultsPtr, flowCount);
            for (int i = 0; i < flowCount; i++)
            {
                ref readonly var r = ref results[i];
                
                // Additional validation could be added here if needed
                // For example, checking that source and target node IDs are non-negative
                
                edgeFlows[i] = new EdgeFlow(
                    new Node(r.source),
                    new Node(r.target),
                    r.flow
                );
            }
        }

        return edgeFlows;
    }

    /// <summary>
    /// Validates that a flow value returned from native code is valid.
    /// </summary>
    /// <param name="flowValue">The flow value to validate.</param>
    /// <exception cref="InvalidOperationException">If the flow value indicates an error.</exception>
    internal static void ValidateFlowValue(double flowValue)
    {
        if (flowValue < 0)
        {
            throw new InvalidOperationException($"Algorithm failed with error code: {flowValue}");
        }

        if (double.IsNaN(flowValue) || double.IsInfinity(flowValue))
        {
            throw new InvalidOperationException($"Algorithm returned invalid flow value: {flowValue}");
        }
    }
}