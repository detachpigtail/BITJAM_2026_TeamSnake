using System.Collections.Generic;

namespace MotionCore2D.Steps
{
    /// <summary>
    /// Implemented by a component that contributes one or more <see cref="IMotionStep"/> instances to
    /// the <see cref="Core.MotionController2D"/> pipeline. This is the open extension point that lets
    /// optional features — in this assembly or another, such as Pro abilities — participate in the
    /// single ordered velocity update without the controller needing compile-time knowledge of them.
    /// </summary>
    /// <remarks>
    /// Provider steps are appended after the built-in pipeline (horizontal, crouch, jump, gravity,
    /// slope), so they observe and may adjust the otherwise-final velocity for the step. The method is
    /// called once during pipeline assembly; the returned steps are reused every frame.
    /// </remarks>
    public interface IMotionStepProvider
    {
        /// <summary>
        /// Returns the steps this component contributes. Return an empty sequence (or <c>null</c>) to
        /// contribute nothing. Null entries in the sequence are ignored.
        /// </summary>
        /// <returns>The contributed steps, in the order they should run.</returns>
        IEnumerable<IMotionStep> GetMotionSteps();
    }
}
