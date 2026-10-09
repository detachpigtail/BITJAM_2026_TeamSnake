using UnityEngine;

namespace MotionCore2D.Sensing
{
    /// <summary>
    /// Immutable snapshot of the ground situation beneath an actor for a single simulation step,
    /// as produced by an <see cref="IGroundSensor"/>.
    /// </summary>
    public readonly struct GroundInfo
    {
        /// <summary>Whether valid ground was detected.</summary>
        public bool IsGrounded { get; }

        /// <summary>Surface normal of the detected ground. Defaults to <see cref="Vector2.up"/> when airborne.</summary>
        public Vector2 Normal { get; }

        /// <summary>Angle of the detected surface from world up, in degrees.</summary>
        public float SlopeAngle { get; }

        /// <summary>Distance from the probe origin to the detected ground, in world units.</summary>
        public float Distance { get; }

        /// <summary>The detected ground collider, or <c>null</c> when airborne.</summary>
        public Collider2D Collider { get; }

        /// <summary>
        /// Initialises a new <see cref="GroundInfo"/>.
        /// </summary>
        /// <param name="isGrounded">Whether valid ground was detected.</param>
        /// <param name="normal">Surface normal of the detected ground.</param>
        /// <param name="slopeAngle">Angle of the surface from world up, in degrees.</param>
        /// <param name="distance">Distance from the probe origin to the ground, in world units.</param>
        /// <param name="collider">The detected ground collider, or <c>null</c>.</param>
        public GroundInfo(bool isGrounded, Vector2 normal, float slopeAngle, float distance, Collider2D collider)
        {
            IsGrounded = isGrounded;
            Normal = normal;
            SlopeAngle = slopeAngle;
            Distance = distance;
            Collider = collider;
        }

        /// <summary>
        /// A canonical "no ground detected" result: not grounded, upward normal, infinite distance.
        /// </summary>
        public static GroundInfo Airborne =>
            new GroundInfo(false, Vector2.up, 0f, float.PositiveInfinity, null);
    }
}
