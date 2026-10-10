namespace MotionCore2D.Sensing
{
    /// <summary>
    /// Abstraction over ground detection. Implementations probe the environment and report the
    /// result as a <see cref="GroundInfo"/>, decoupling motion logic from the probing technique
    /// (raycast, box cast, one-way platforms, and so on).
    /// </summary>
    public interface IGroundSensor
    {
        /// <summary>
        /// Probes the environment and returns the ground situation for the current step.
        /// </summary>
        /// <returns>The detected <see cref="GroundInfo"/>, or <see cref="GroundInfo.Airborne"/> when no ground is found.</returns>
        GroundInfo Sample();
    }
}
