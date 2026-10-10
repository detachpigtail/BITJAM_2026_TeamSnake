using UnityEngine;

namespace MotionCore2D.Steps
{
    /// <summary>
    /// Applies manual gravity from the movement profile. While grounded and not rising, vertical
    /// velocity is held at rest. While airborne (or rising out of the ground after a jump) gravity
    /// is integrated, with a heavier multiplier during descent and a clamp to the maximum fall
    /// speed.
    /// </summary>
    internal sealed class GravityStep : IMotionStep
    {
        public void Execute(MotionContext context)
        {
            var profile = context.Profile;
            Vector2 velocity = context.State.Velocity;

            if (context.State.IsGrounded && velocity.y <= 0f)
            {
                velocity.y = 0f;
            }
            else
            {
                float gravity = profile.Gravity;
                if (velocity.y < 0f)
                {
                    gravity *= profile.FallGravityMultiplier;
                }

                velocity.y -= gravity * context.DeltaTime;
                if (velocity.y < -profile.MaxFallSpeed)
                {
                    velocity.y = -profile.MaxFallSpeed;
                }
            }

            context.State.Velocity = velocity;
        }
    }
}
