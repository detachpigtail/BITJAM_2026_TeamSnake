namespace MotionCore2D.Crouch
{
    /// <summary>
    /// The physics operations the crouch step needs from the actor's body: a headroom test that
    /// reports whether the actor can stand up, and the collider resize that applies the crouch.
    /// </summary>
    /// <remarks>
    /// Keeping these behind an interface lets the <see cref="CrouchStep"/> stay free of Unity
    /// physics, so the crouch decision logic in <see cref="CrouchStateMachine"/> remains the only
    /// place a crouch is decided. <see cref="CrouchController2D"/> is the production implementation.
    /// </remarks>
    internal interface ICrouchBody
    {
        /// <summary>
        /// Whether the actor currently has room to stand at full height. Returns <c>true</c> when
        /// already standing or when the space above a crouched collider is clear; <c>false</c> when
        /// a ceiling would be intersected by standing up.
        /// </summary>
        bool HasHeadroom { get; }

        /// <summary>
        /// Sets the actor's collider to the crouched or standing size. Implementations keep the feet
        /// planted and are idempotent, so calling with the current state every step is cheap.
        /// </summary>
        /// <param name="crouched"><c>true</c> to lower to the crouched size; <c>false</c> to restore standing size.</param>
        /// <param name="heightFactor">Crouched height as a fraction of the standing height, in the range 0.1 to 1.</param>
        void SetCrouched(bool crouched, float heightFactor);
    }
}
