using UnityEngine;

namespace MotionCore2D.Steps
{
    /// <summary>
    /// Drives horizontal velocity toward the input-derived target speed, accelerating while the
    /// player pushes a direction and decelerating when input is released or reversed. While airborne
    /// the rate is scaled by the profile's air-control factor.
    /// </summary>
    internal sealed class HorizontalMovementStep : IMotionStep
    {
        public void Execute(MotionContext context)
        {
            var profile = context.Profile;
            Vector2 velocity = context.State.Velocity;

            float target = Mathf.Clamp(context.Input.Horizontal, -1f, 1f) * profile.MaxSpeed;

            // Deceleration governs every loss of speed, whether the player let go or turned around.
            // Charging a reversal at the acceleration rate would make Deceleration control only half
            // of what it claims to, and would leave a heavy actor turning on a dime.
            bool releasing = Mathf.Approximately(target, 0f);
            bool reversing = target * velocity.x < 0f;
            float rate = releasing || reversing ? profile.Deceleration : profile.Acceleration;

            if (!context.State.IsGrounded)
            {
                rate *= profile.AirControlFactor;
            }

            velocity.x = Mathf.MoveTowards(velocity.x, target, rate * context.DeltaTime);
            context.State.Velocity = velocity;
        }
    }
}
