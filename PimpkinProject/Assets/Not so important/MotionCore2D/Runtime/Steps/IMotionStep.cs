namespace MotionCore2D.Steps
{
    /// <summary>
    /// A single, ordered unit of motion processing. Each step reads from the supplied
    /// <see cref="MotionContext"/> and writes its contribution into the shared
    /// <see cref="MotionState"/>. This is the primary extension point: built-in and add-on
    /// abilities are implemented as steps inserted into the pipeline.
    /// </summary>
    /// <remarks>
    /// Steps are expected to be deterministic given their inputs and to confine their effects to
    /// <see cref="MotionContext.State"/>; they must not mutate <see cref="MotionContext.Profile"/>.
    /// Execution order is significant and defined by the orchestrator.
    /// </remarks>
    public interface IMotionStep
    {
        /// <summary>
        /// Executes this step for the current simulation step.
        /// </summary>
        /// <param name="context">The data for this step, including the mutable state to update.</param>
        void Execute(MotionContext context);
    }
}
