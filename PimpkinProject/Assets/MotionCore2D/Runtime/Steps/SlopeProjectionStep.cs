using UnityEngine;

namespace MotionCore2D.Steps
{
    /// <summary>
    /// Keeps a grounded actor moving along walkable slopes. When standing on a slope within the
    /// profile's maximum angle, the horizontal speed is reprojected along the surface tangent so
    /// the actor follows the ground instead of launching off descents or burrowing into ascents.
    /// Flat ground, steep (non-walkable) ground, and rising actors are left untouched.
    /// </summary>
    internal sealed class SlopeProjectionStep : IMotionStep
    {
        private const float FlatAngleThreshold = 0.01f;

        public void Execute(MotionContext context)
        {
            if (!context.State.IsGrounded)
            {
                return;
            }

            Vector2 velocity = context.State.Velocity;
            if (velocity.y > 0f)
            {
                return;
            }

            float angle = context.Ground.SlopeAngle;
            if (angle <= FlatAngleThreshold || angle > context.Profile.MaxSlopeAngle)
            {
                return;
            }

            Vector2 normal = context.Ground.Normal;
            Vector2 tangent = new Vector2(normal.y, -normal.x);
            context.State.Velocity = tangent * velocity.x;
        }
    }
}
