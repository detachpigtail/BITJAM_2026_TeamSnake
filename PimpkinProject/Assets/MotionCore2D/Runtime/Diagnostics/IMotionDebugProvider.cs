using MotionCore2D.Sensing;

namespace MotionCore2D.Diagnostics
{
    /// <summary>
    /// Read-only view of an actor's current motion data, exposed for diagnostic consumers such as
    /// gizmo visualisers. Decouples debug tooling from the orchestrating component so that neither
    /// depends on the other's internals.
    /// </summary>
    public interface IMotionDebugProvider
    {
        /// <summary>The actor's current runtime state.</summary>
        MotionState State { get; }

        /// <summary>The ground information resolved for the current step.</summary>
        GroundInfo Ground { get; }
    }
}
