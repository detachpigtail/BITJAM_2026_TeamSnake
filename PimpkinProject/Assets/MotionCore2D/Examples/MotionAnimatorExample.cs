// Animator lives in Unity's Animation module, which a trimmed-down 2D project may not have
// installed. Guarding the example keeps that module out of the package's declared dependencies:
// MotionCore itself never needs it, and only this one example does.
#if MOTIONCORE_ANIMATION_MODULE
using MotionCore2D.Core;
using MotionCore2D.Jump;
using UnityEngine;

namespace MotionCore2D.Examples
{
    /// <summary>
    /// Example that drives an <see cref="Animator"/> and a sprite's facing from the actor's published
    /// <see cref="MotionState"/>. Thin glue, not an animation system: copy it into your own project
    /// and rename the parameters to match your controller.
    /// </summary>
    /// <remarks>
    /// The point of the example is that animation never needs to inspect the Rigidbody2D or duplicate
    /// any movement logic. Everything it needs is already resolved on <see cref="MotionState"/> once
    /// per fixed step, which keeps the visuals in step with the simulation.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Examples/Motion Animator Example")]
    public sealed class MotionAnimatorExample : MonoBehaviour
    {
        [SerializeField, Tooltip("Animator to drive. Found on this object or its children when empty.")]
        private Animator _animator;

        [SerializeField, Tooltip("Sprite to flip so the actor faces its direction of travel. Optional.")]
        private SpriteRenderer _sprite;

        [Header("Parameter names")]
        [SerializeField, Tooltip("Float parameter set to absolute horizontal speed, in units per second.")]
        private string _speedParameter = "Speed";

        [SerializeField, Tooltip("Float parameter set to signed vertical speed, in units per second.")]
        private string _verticalSpeedParameter = "VerticalSpeed";

        [SerializeField, Tooltip("Bool parameter set while the actor is on walkable ground.")]
        private string _groundedParameter = "Grounded";

        [SerializeField, Tooltip("Bool parameter set while a jump is carrying the actor upward.")]
        private string _jumpingParameter = "Jumping";

        [SerializeField, Tooltip("Bool parameter set while the actor is crouching.")]
        private string _crouchingParameter = "Crouching";

        [SerializeField, Tooltip("Trigger fired on the step a jump launches. Requires a Jump Controller 2D.")]
        private string _jumpTrigger = "Jump";

        [SerializeField, Tooltip("Trigger fired when the actor lands.")]
        private string _landTrigger = "Land";

        [Header("Facing")]
        [SerializeField, Min(0f), Tooltip("Horizontal speed below which facing is left alone, so the actor does not flicker when nearly still.")]
        private float _facingDeadzone = 0.1f;

        private MotionController2D _controller;
        private JumpController2D _jump;

        private void Awake()
        {
            _controller = GetComponent<MotionController2D>();
            _jump = GetComponent<JumpController2D>();

            if (_animator == null)
            {
                _animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
            }

            if (_sprite == null)
            {
                _sprite = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            }

            if (_controller == null)
            {
                Debug.LogError(
                    $"{nameof(MotionAnimatorExample)} on '{name}' needs a {nameof(MotionController2D)} on the same object to read motion state from.",
                    this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.Landed += OnLanded;
            }

            if (_jump != null)
            {
                _jump.Jumped += OnJumped;
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.Landed -= OnLanded;
            }

            if (_jump != null)
            {
                _jump.Jumped -= OnJumped;
            }
        }

        private void Update()
        {
            MotionState state = _controller.State;

            if (_animator != null)
            {
                SetFloat(_speedParameter, Mathf.Abs(state.Velocity.x));
                SetFloat(_verticalSpeedParameter, state.Velocity.y);
                SetBool(_groundedParameter, state.IsGrounded);
                SetBool(_jumpingParameter, state.IsJumping);
                SetBool(_crouchingParameter, state.IsCrouching);
            }

            if (_sprite != null && Mathf.Abs(state.Velocity.x) > _facingDeadzone)
            {
                _sprite.flipX = state.Velocity.x < 0f;
            }
        }

        private void OnJumped() => SetTrigger(_jumpTrigger);

        private void OnLanded() => SetTrigger(_landTrigger);

        // Each setter tolerates an unset name so the example works with a controller that only
        // defines some of these parameters. Unity logs its own warning for a name that is set but
        // missing from the controller, which is the signal you want while wiring things up.
        private void SetFloat(string parameter, float value)
        {
            if (!string.IsNullOrEmpty(parameter))
            {
                _animator.SetFloat(parameter, value);
            }
        }

        private void SetBool(string parameter, bool value)
        {
            if (!string.IsNullOrEmpty(parameter))
            {
                _animator.SetBool(parameter, value);
            }
        }

        private void SetTrigger(string parameter)
        {
            if (_animator != null && !string.IsNullOrEmpty(parameter))
            {
                _animator.SetTrigger(parameter);
            }
        }
    }
}
#endif
