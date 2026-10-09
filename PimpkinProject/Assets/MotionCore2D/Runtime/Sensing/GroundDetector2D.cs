using UnityEngine;
#if UNITY_EDITOR
using MotionCore2D.Diagnostics;
#endif

namespace MotionCore2D.Sensing
{
    /// <summary>
    /// Detects ground beneath a 2D actor with a single downward box cast and reports the result as
    /// a <see cref="GroundInfo"/>: whether ground was found, the surface normal, the slope angle,
    /// and the distance to contact.
    /// </summary>
    /// <remarks>
    /// This component is a pure sensor. It reads the physics scene and produces information; it
    /// never moves the actor, alters velocity, or makes movement decisions. In particular it does
    /// not judge whether a slope is walkable — it reports the raw angle and leaves that policy to
    /// the consumer (for example a movement step comparing against
    /// <see cref="Configuration.MovementProfile.MaxSlopeAngle"/>). The sensor is stateless: each
    /// <see cref="Sample"/> performs an independent query.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Sensing/Ground Detector 2D")]
    [DisallowMultipleComponent]
    public sealed class GroundDetector2D : MonoBehaviour, IGroundSensor
    {
        private const float NormalEpsilon = 1e-6f;

        [SerializeField, Tooltip("Physics layers treated as ground. Must exclude the actor's own collider.")]
        private LayerMask _groundLayers = 0;

        [SerializeField, Tooltip("Local offset from the transform to the centre of the probe (typically at the actor's feet).")]
        private Vector2 _originOffset = new Vector2(0f, -0.5f);

        [SerializeField, Tooltip("Width and height of the box used to probe for ground, in world units.")]
        private Vector2 _probeSize = new Vector2(0.4f, 0.1f);

        [SerializeField, Min(0f), Tooltip("How far below the probe origin to search for ground, in world units.")]
        private float _probeDistance = 0.15f;

        [SerializeField, Min(0f), Tooltip("Small inset above the origin from which the cast begins, avoiding starts already inside the ground.")]
        private float _skinWidth = 0.02f;

        /// <inheritdoc />
        public GroundInfo Sample()
        {
            Vector2 origin = ProbeOrigin();
            float castDistance = _probeDistance + _skinWidth;

            RaycastHit2D hit = Physics2D.BoxCast(
                origin, _probeSize, 0f, Vector2.down, castDistance, _groundLayers);

            if (hit.collider == null)
            {
                return GroundInfo.Airborne;
            }

            // A cast that begins already overlapping a collider reports a zero distance and, with it,
            // a degenerate normal. Substituting world up keeps consumers from reasoning about a
            // zero-length normal: the actor is flush against the surface, so flat is the honest
            // reading, and a tangent built from it stays unit length.
            Vector2 normal = hit.normal.sqrMagnitude > NormalEpsilon ? hit.normal.normalized : Vector2.up;

            float distance = Mathf.Max(0f, hit.distance - _skinWidth);
            float slopeAngle = Vector2.Angle(normal, Vector2.up);
            return new GroundInfo(true, normal, slopeAngle, distance, hit.collider);
        }

        private Vector2 ProbeOrigin()
        {
            return (Vector2)transform.position + _originOffset + (Vector2.up * _skinWidth);
        }

        private void OnValidate()
        {
            _probeSize = new Vector2(Mathf.Max(0f, _probeSize.x), Mathf.Max(0f, _probeSize.y));
            _probeDistance = Mathf.Max(0f, _probeDistance);
            _skinWidth = Mathf.Max(0f, _skinWidth);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 origin = ProbeOrigin();
            GroundInfo info = Sample();
            Color probeColor = info.IsGrounded ? Color.green : Color.red;

            MotionGizmos.DrawBoxCast(origin, _probeSize, _probeDistance + _skinWidth, probeColor);

            if (info.IsGrounded)
            {
                Vector2 contact = origin + (Vector2.down * (info.Distance + _skinWidth));
                MotionGizmos.DrawNormal(contact, info.Normal, Color.yellow);
            }
        }
#endif
    }
}
