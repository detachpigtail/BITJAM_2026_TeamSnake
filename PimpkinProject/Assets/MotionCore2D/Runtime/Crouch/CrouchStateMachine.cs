namespace MotionCore2D.Crouch
{
    /// <summary>
    /// Pure, engine-independent decision logic for crouching. Given the per-step situation it
    /// decides whether the actor is crouching, remembering that an actor which wants to stand but
    /// is blocked by a low ceiling must remain crouched until it has the headroom to rise.
    /// </summary>
    /// <remarks>
    /// The machine owns only the single bit of state the feature needs (whether it is currently
    /// crouching). It performs no input reading, no physics, and no collider work; it consumes
    /// <see cref="Inputs"/> and returns a <see cref="Result"/> for the caller to apply. This keeps
    /// the logic fully unit testable. The actual headroom probe and collider resize live in the
    /// component that drives this machine.
    /// </remarks>
    public sealed class CrouchStateMachine
    {
        private bool _crouching;

        /// <summary>Whether the actor was crouching as of the last completed evaluation.</summary>
        public bool IsCrouching => _crouching;

        /// <summary>
        /// Evaluates the crouch decision for a single simulation step.
        /// </summary>
        /// <param name="inputs">The situation for this step.</param>
        /// <returns>Whether the actor should be crouching after this step.</returns>
        public Result Evaluate(in Inputs inputs)
        {
            // Intent to crouch only applies on the ground; an airborne actor is never newly crouched
            // by holding the control.
            bool wantsCrouch = inputs.CrouchHeld && inputs.IsGrounded;

            if (wantsCrouch)
            {
                _crouching = true;
            }
            else if (_crouching)
            {
                // Release is only honoured when there is room to stand back up; otherwise the actor
                // stays crouched under the obstruction.
                _crouching = inputs.BlockedAbove;
            }

            return new Result(_crouching);
        }

        /// <summary>Clears crouch state, returning the machine to standing.</summary>
        public void Reset()
        {
            _crouching = false;
        }

        /// <summary>
        /// Immutable description of the situation for a single crouch evaluation.
        /// </summary>
        public readonly struct Inputs
        {
            /// <summary>Whether the crouch control is currently held.</summary>
            public bool CrouchHeld { get; }

            /// <summary>Whether the actor is grounded this step.</summary>
            public bool IsGrounded { get; }

            /// <summary>Whether an obstacle overhead prevents the actor from standing up this step.</summary>
            public bool BlockedAbove { get; }

            /// <summary>Initialises a new <see cref="Inputs"/>.</summary>
            /// <param name="crouchHeld">Whether the crouch control is currently held.</param>
            /// <param name="isGrounded">Whether the actor is grounded this step.</param>
            /// <param name="blockedAbove">Whether an obstacle overhead prevents standing up.</param>
            public Inputs(bool crouchHeld, bool isGrounded, bool blockedAbove)
            {
                CrouchHeld = crouchHeld;
                IsGrounded = isGrounded;
                BlockedAbove = blockedAbove;
            }
        }

        /// <summary>
        /// Immutable outcome of a crouch evaluation.
        /// </summary>
        public readonly struct Result
        {
            /// <summary>Initialises a new <see cref="Result"/>.</summary>
            /// <param name="isCrouching">Whether the actor should be crouching.</param>
            public Result(bool isCrouching)
            {
                IsCrouching = isCrouching;
            }

            /// <summary>Whether the actor should be crouching after this step.</summary>
            public bool IsCrouching { get; }
        }
    }
}
