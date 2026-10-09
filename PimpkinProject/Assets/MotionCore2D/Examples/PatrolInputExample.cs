using MotionCore2D.Core;
using MotionCore2D.Input;
using UnityEngine;

namespace MotionCore2D.Examples
{
    /// <summary>
    /// Example that drives an actor along a patrol route by writing to a
    /// <see cref="ScriptedMotionInput"/>. It shows how a non-player actor reuses the whole movement
    /// system — acceleration, gravity, slopes, jumping — without any special AI support in the
    /// controller.
    /// </summary>
    /// <remarks>
    /// The actor walks back and forth between two x positions, turning early when it runs out of
    /// ground ahead so it does not walk off a ledge. Nothing here touches the Rigidbody2D: it only
    /// sets the same intent a player's controls would produce, which is why the actor moves with the
    /// same feel as the player.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Examples/Patrol Input Example")]
    [RequireComponent(typeof(ScriptedMotionInput))]
    public sealed class PatrolInputExample : MonoBehaviour
    {
        [Header("Route")]
        [SerializeField, Tooltip("How far to either side of the starting position the actor patrols, in world units.")]
        private float _patrolRadius = 4f;

        [SerializeField, Range(0f, 1f), Tooltip("Fraction of full speed to walk at. 1 is the profile's Max Speed.")]
        private float _walkSpeedFraction = 0.6f;

        [SerializeField, Tooltip("Start by walking right rather than left.")]
        private bool _startFacingRight = true;

        [Header("Ledge safety")]
        [SerializeField, Tooltip("Turn around when there is no ground ahead, instead of walking off the edge.")]
        private bool _turnAtLedges = true;

        [SerializeField, Tooltip("Layers counted as ground by the ledge check. Match the actor's Ground Detector 2D.")]
        private LayerMask _groundLayers = 0;

        [SerializeField, Tooltip("How far ahead of the actor to check for ground, in world units.")]
        private float _lookAheadDistance = 0.6f;

        [SerializeField, Tooltip("How far down to check for ground at the look-ahead point, in world units.")]
        private float _ledgeProbeDepth = 1.5f;

        private ScriptedMotionInput _input;
        private MotionController2D _controller;
        private float _originX;
        private int _direction;

        private void Awake()
        {
            _input = GetComponent<ScriptedMotionInput>();
            _controller = GetComponent<MotionController2D>();
            _originX = transform.position.x;
            _direction = _startFacingRight ? 1 : -1;
        }

        private void OnDisable()
        {
            // Hand back a clean slate, so a disabled patroller does not leave the actor leaning in
            // the direction it happened to be walking.
            if (_input != null)
            {
                _input.Clear();
            }
        }

        private void Update()
        {
            if (ShouldTurn())
            {
                _direction = -_direction;
            }

            _input.Horizontal = _direction * _walkSpeedFraction;
        }

        private bool ShouldTurn()
        {
            float offset = transform.position.x - _originX;
            if ((_direction > 0 && offset >= _patrolRadius) || (_direction < 0 && offset <= -_patrolRadius))
            {
                return true;
            }

            return _turnAtLedges && IsGroundMissingAhead();
        }

        private bool IsGroundMissingAhead()
        {
            // Only meaningful while standing on something: mid-air there is nothing ahead to leave.
            if (_groundLayers == 0 || (_controller != null && !_controller.State.IsGrounded))
            {
                return false;
            }

            Vector2 ahead = (Vector2)transform.position + (Vector2.right * (_direction * _lookAheadDistance));
            return !Physics2D.Raycast(ahead, Vector2.down, _ledgeProbeDepth, _groundLayers);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            float originX = Application.isPlaying ? _originX : transform.position.x;
            float y = transform.position.y;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(originX - _patrolRadius, y, 0f), new Vector3(originX + _patrolRadius, y, 0f));

            if (!_turnAtLedges)
            {
                return;
            }

            int direction = Application.isPlaying ? _direction : (_startFacingRight ? 1 : -1);
            var ahead = new Vector3(transform.position.x + (direction * _lookAheadDistance), y, 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(ahead, ahead + (Vector3.down * _ledgeProbeDepth));
        }
#endif
    }
}
