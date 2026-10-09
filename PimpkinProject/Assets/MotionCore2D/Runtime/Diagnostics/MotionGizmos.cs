#if UNITY_EDITOR
using MotionCore2D.Sensing;
using UnityEngine;

namespace MotionCore2D.Diagnostics
{
    /// <summary>
    /// Stateless, editor-only helpers for drawing motion diagnostics with <see cref="Gizmos"/>.
    /// Compiled out of player builds.
    /// </summary>
    public static class MotionGizmos
    {
        /// <summary>
        /// Draws the velocity vector as a ray originating at the given world position.
        /// </summary>
        /// <param name="origin">World-space origin of the vector.</param>
        /// <param name="velocity">Velocity to draw, in world units per second.</param>
        /// <param name="color">Colour of the gizmo.</param>
        public static void DrawVelocity(Vector3 origin, Vector2 velocity, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(origin, velocity);
        }

        /// <summary>
        /// Draws the ground contact normal when ground is detected.
        /// </summary>
        /// <param name="origin">World-space origin of the probe.</param>
        /// <param name="ground">The ground information to visualise.</param>
        /// <param name="color">Colour of the gizmo.</param>
        public static void DrawGround(Vector3 origin, GroundInfo ground, Color color)
        {
            if (!ground.IsGrounded)
            {
                return;
            }

            Gizmos.color = color;
            Gizmos.DrawRay(origin, ground.Normal);
        }

        /// <summary>
        /// Draws a swept box, visualising the start and end of a downward box cast.
        /// </summary>
        /// <param name="origin">World-space centre of the box at the start of the cast.</param>
        /// <param name="size">Box dimensions, in world units.</param>
        /// <param name="distance">Downward cast distance, in world units.</param>
        /// <param name="color">Colour of the gizmo.</param>
        public static void DrawBoxCast(Vector2 origin, Vector2 size, float distance, Color color)
        {
            Gizmos.color = color;
            Vector2 end = origin + (Vector2.down * distance);
            Gizmos.DrawWireCube(origin, size);
            Gizmos.DrawWireCube(end, size);
            Gizmos.DrawLine(origin, end);
        }

        /// <summary>
        /// Draws a surface normal at a contact point.
        /// </summary>
        /// <param name="point">World-space contact point.</param>
        /// <param name="normal">Surface normal to draw.</param>
        /// <param name="color">Colour of the gizmo.</param>
        public static void DrawNormal(Vector2 point, Vector2 normal, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(point, 0.04f);
            Gizmos.DrawRay(point, normal);
        }

        /// <summary>
        /// Draws a small marker whose colour reflects whether the actor is grounded.
        /// </summary>
        /// <param name="origin">World-space position of the marker.</param>
        /// <param name="isGrounded">Whether the actor is grounded.</param>
        /// <param name="groundedColor">Colour used when grounded.</param>
        /// <param name="airborneColor">Colour used when airborne.</param>
        public static void DrawGroundedMarker(Vector3 origin, bool isGrounded, Color groundedColor, Color airborneColor)
        {
            Gizmos.color = isGrounded ? groundedColor : airborneColor;
            Gizmos.DrawWireSphere(origin, 0.1f);
        }
    }
}
#endif
