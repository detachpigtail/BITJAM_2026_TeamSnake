using MotionCore2D.Core;
using MotionCore2D.Jump;
using UnityEngine;
using UnityEngine.Events;

namespace MotionCore2D.Events
{
    /// <summary>
    /// Surfaces an actor's motion events as <see cref="UnityEvent"/>s so that audio, particles,
    /// animation triggers, and camera shake can be wired up in the Inspector without writing any
    /// code. It is a separate, optional component and changes no movement behaviour.
    /// </summary>
    /// <remarks>
    /// The C# events on <see cref="MotionController2D"/> and <see cref="JumpController2D"/> remain the
    /// right choice from script. This component exists for the designer-facing half of the workflow:
    /// drop it on the actor, drag an AudioSource into <c>Jumped</c>, and the jump has a sound.
    /// <para>
    /// Crouch transitions are polled rather than subscribed, because crouching publishes state rather
    /// than an event; the relay compares <see cref="MotionState.IsCrouching"/> between steps.
    /// </para>
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Events/Motion Event Relay")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MotionController2D))]
    // Ordered after the controller so the crouch comparison reads the state the pipeline just
    // produced, rather than trailing it by a step.
    [DefaultExecutionOrder(100)]
    public sealed class MotionEventRelay : MonoBehaviour
    {
        /// <summary>A <see cref="UnityEvent"/> carrying a single speed value, in units per second.</summary>
        [System.Serializable]
        public sealed class SpeedEvent : UnityEvent<float>
        {
        }

        [Header("Ground")]
        [SerializeField, Tooltip("Raised when the actor touches down after being airborne. The float is the downward speed it landed at, so feedback can scale with the drop.")]
        private SpeedEvent _landed = new SpeedEvent();

        [SerializeField, Tooltip("Raised when the actor leaves the ground, whether by jumping, walking off a ledge, or being pushed.")]
        private UnityEvent _leftGround = new UnityEvent();

        [Header("Jump")]
        [SerializeField, Tooltip("Raised on the step a jump launches. Requires a Jump Controller 2D on this object.")]
        private UnityEvent _jumped = new UnityEvent();

        [Header("Crouch")]
        [SerializeField, Tooltip("Raised when the actor starts crouching. Requires a Crouch Controller 2D on this object.")]
        private UnityEvent _crouchStarted = new UnityEvent();

        [SerializeField, Tooltip("Raised when the actor stands back up, which can be well after the control is released if a ceiling was in the way.")]
        private UnityEvent _crouchEnded = new UnityEvent();

        private MotionController2D _controller;
        private JumpController2D _jump;
        private bool _wasCrouching;

        /// <summary>Raised when the actor lands, carrying the downward speed of the impact.</summary>
        public SpeedEvent Landed => _landed;

        /// <summary>Raised when the actor leaves the ground.</summary>
        public UnityEvent LeftGround => _leftGround;

        /// <summary>Raised on the step a jump launches.</summary>
        public UnityEvent Jumped => _jumped;

        /// <summary>Raised when the actor starts crouching.</summary>
        public UnityEvent CrouchStarted => _crouchStarted;

        /// <summary>Raised when the actor stands back up.</summary>
        public UnityEvent CrouchEnded => _crouchEnded;

        private void Awake()
        {
            _controller = GetComponent<MotionController2D>();
            _jump = GetComponent<JumpController2D>();
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.Landed += OnLanded;
                _controller.LeftGround += OnLeftGround;
                _wasCrouching = _controller.State.IsCrouching;
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
                _controller.LeftGround -= OnLeftGround;
            }

            if (_jump != null)
            {
                _jump.Jumped -= OnJumped;
            }
        }

        private void FixedUpdate()
        {
            if (_controller == null)
            {
                return;
            }

            bool crouching = _controller.State.IsCrouching;
            if (crouching == _wasCrouching)
            {
                return;
            }

            _wasCrouching = crouching;
            if (crouching)
            {
                _crouchStarted.Invoke();
            }
            else
            {
                _crouchEnded.Invoke();
            }
        }

        private void OnLanded() => _landed.Invoke(_controller.State.LandingImpactSpeed);

        private void OnLeftGround() => _leftGround.Invoke();

        private void OnJumped() => _jumped.Invoke();
    }
}
