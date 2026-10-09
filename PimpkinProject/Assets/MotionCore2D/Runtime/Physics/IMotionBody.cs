using UnityEngine;

namespace MotionCore2D.Physics
{
    /// <summary>
    /// Abstraction over the physical body that the motion system drives. Isolates the rest of the
    /// system from a specific physics backend (for example <c>Rigidbody2D</c>), confining
    /// engine-version and body-type differences to a single implementation.
    /// </summary>
    public interface IMotionBody
    {
        /// <summary>
        /// The body's linear velocity, in world units per second.
        /// </summary>
        Vector2 Velocity { get; set; }

        /// <summary>
        /// The body's current world-space position.
        /// </summary>
        Vector2 Position { get; }
    }
}
