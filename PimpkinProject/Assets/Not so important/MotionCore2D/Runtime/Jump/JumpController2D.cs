using System;
using MotionCore2D.Steps;
using UnityEngine;

namespace MotionCore2D.Jump
{
    /// <summary>
    /// Adds jumping to an actor: a standard jump, variable jump height, coyote time, and jump
    /// buffering. It is a separate, optional component — an actor without it simply cannot jump.
    /// </summary>
    /// <remarks>
    /// This component owns the jump logic but does not drive the simulation itself. It exposes a
    /// <see cref="Step"/> that <see cref="Core.MotionController2D"/> inserts into its pipeline, so
    /// jumping participates in the single, ordered velocity update rather than competing with the
    /// controller for the body. All tuning comes from the controller's movement profile.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Jump/Jump Controller 2D")]
    [DisallowMultipleComponent]
    public sealed class JumpController2D : MonoBehaviour
    {
        private JumpStep _step;

        /// <summary>
        /// Raised on the fixed step a jump launches, before the body is written. Handy for jump
        /// audio, particles, or squash-and-stretch without polling for a velocity change.
        /// </summary>
        public event Action Jumped;

        /// <summary>
        /// The jump pipeline step contributed by this component. Consumed by
        /// <see cref="Core.MotionController2D"/> during pipeline assembly.
        /// </summary>
        public IMotionStep Step => JumpStep;

        /// <summary>Whether a jump is currently carrying the actor upward.</summary>
        public bool IsRising => JumpStep.IsRising;

        /// <summary>
        /// Clears coyote time, the jump buffer, and any in-progress ascent. Call it after teleporting
        /// or respawning the actor so a jump queued before the move cannot fire after it.
        /// </summary>
        public void ResetJumpState() => JumpStep.Reset();

        /// <summary>
        /// Expires coyote time and the jump buffer for <paramref name="deltaTime"/> seconds without
        /// evaluating a jump. <see cref="Core.MotionController2D"/> calls this for every step its
        /// pipeline is suspended, so the assists cannot be banked while something else owns the body.
        /// </summary>
        /// <param name="deltaTime">Elapsed time to charge against the windows, in seconds.</param>
        public void AdvanceAssistTimers(float deltaTime) => JumpStep.AdvanceAssistTimers(deltaTime);

        private JumpStep JumpStep =>
            _step ?? (_step = new JumpStep(() => Jumped?.Invoke()));
    }
}
