using UnityEngine;

namespace MotionCore2D
{
    /// <summary>
    /// Mutable, per-instance runtime state describing the kinematic and contact situation of a
    /// single 2D motion actor for the current simulation step.
    /// </summary>
    /// <remarks>
    /// This type is a plain data container with no behaviour. It is owned by the orchestrating
    /// component and shared (by reference) with the motion pipeline so that steps can read and
    /// write the evolving state. Keeping it free of Unity component dependencies allows the
    /// decision logic that consumes it to be unit tested without entering Play mode.
    /// </remarks>
    public sealed class MotionState
    {
        /// <summary>
        /// Current linear velocity, in world units per second.
        /// </summary>
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// Whether the actor is resting on valid ground as resolved for the current step.
        /// </summary>
        public bool IsGrounded { get; set; }

        /// <summary>
        /// The value of <see cref="IsGrounded"/> as resolved during the previous completed step.
        /// Useful for detecting ground-contact transitions (take-off and landing).
        /// </summary>
        public bool WasGrounded { get; set; }

        /// <summary>
        /// Whether the actor is crouching as resolved for the current step. Set by the optional
        /// crouch feature; remains <c>false</c> for actors with no crouch component. Consumers such
        /// as animation can read it to drive a crouched pose.
        /// </summary>
        public bool IsCrouching { get; set; }

        /// <summary>
        /// Whether a jump is currently carrying the actor upward. Set by the optional jump feature
        /// and cleared at the apex, when the jump is cut short by an early release, and on landing;
        /// it remains <c>false</c> for actors with no jump component. This distinguishes a rising
        /// jump from any other upward motion, which a velocity check alone cannot do.
        /// </summary>
        public bool IsJumping { get; set; }

        /// <summary>
        /// How fast the actor was falling, in world units per second, at the moment it last landed.
        /// Always zero or positive; zero until the first landing.
        /// </summary>
        /// <remarks>
        /// Captured before gravity zeroes vertical velocity against the ground, so it survives to be
        /// read from a landing handler. Scale landing feedback with it — dust size, impact volume,
        /// camera shake, squash depth — so a short step down reads differently from a long drop.
        /// </remarks>
        public float LandingImpactSpeed { get; set; }
    }
}
