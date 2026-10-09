using UnityEngine;

namespace MotionCore2D.Diagnostics
{
    /// <summary>
    /// Editor-only visualiser that draws the motion data published by an
    /// <see cref="IMotionDebugProvider"/> on the same GameObject. All fields and drawing are
    /// compiled out of player builds, so the component has no runtime cost when shipped; the type
    /// itself is preserved in every build so scene references remain intact.
    /// </summary>
    [AddComponentMenu("MotionCore 2D/Debug/Motion Debug View")]
    [DisallowMultipleComponent]
    public sealed class MotionDebugView : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField, Tooltip("Marker colour while grounded.")]
        private Color _groundedColor = Color.green;

        [SerializeField, Tooltip("Marker colour while airborne.")]
        private Color _airborneColor = Color.red;

        [SerializeField, Tooltip("Colour of the velocity vector.")]
        private Color _velocityColor = Color.cyan;

        [SerializeField, Tooltip("Colour of the ground normal.")]
        private Color _normalColor = Color.yellow;

        private IMotionDebugProvider _provider;

        private void OnEnable()
        {
            _provider = GetComponent<IMotionDebugProvider>();
        }

        private void OnDrawGizmos()
        {
            if (_provider == null)
            {
                _provider = GetComponent<IMotionDebugProvider>();
            }

            if (_provider == null)
            {
                return;
            }

            Vector3 origin = transform.position;
            MotionState state = _provider.State;

            if (state != null)
            {
                MotionGizmos.DrawVelocity(origin, state.Velocity, _velocityColor);
                MotionGizmos.DrawGroundedMarker(origin, state.IsGrounded, _groundedColor, _airborneColor);
            }

            MotionGizmos.DrawGround(origin, _provider.Ground, _normalColor);
        }
#endif
    }
}
