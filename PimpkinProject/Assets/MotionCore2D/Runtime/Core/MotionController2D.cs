using System;
using System.Collections.Generic;
using MotionCore2D.Configuration;
using MotionCore2D.Crouch;
using MotionCore2D.Diagnostics;
using MotionCore2D.Input;
using MotionCore2D.Jump;
using MotionCore2D.Physics;
using MotionCore2D.Sensing;
using MotionCore2D.Steps;
using UnityEngine;

namespace MotionCore2D.Core
{
    /// <summary>
    /// The orchestrator for a 2D motion actor. Each fixed step it samples ground, synchronises the
    /// shared <see cref="MotionState"/> from the body, runs an ordered pipeline of motion steps,
    /// and writes the resulting velocity back to the body exactly once.
    /// </summary>
    /// <remarks>
    /// The controller contains no movement maths of its own; each concern (horizontal movement,
    /// crouching, gravity, jumping, slope projection) lives in its own <see cref="IMotionStep"/> and
    /// runs in that order. Crouching and jumping are contributed by the optional
    /// <see cref="CrouchController2D"/> and <see cref="JumpController2D"/> on the same GameObject,
    /// and any component implementing <see cref="IMotionStepProvider"/> may append further steps.
    /// Because the controller is the only component that writes to the body, there is a single,
    /// well-defined velocity update each step.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Motion Controller 2D")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterMotor2D))]
    public sealed class MotionController2D : MonoBehaviour, IMotionDebugProvider
    {
        [SerializeField, Tooltip("Tuning data for movement, jumping, gravity, and slopes.")]
        private MovementProfile _profile;

        private readonly MotionState _state = new MotionState();
        private readonly List<IMotionStep> _pipeline = new List<IMotionStep>();

        private IMotionInput _input;
        private IGroundSensor _groundSensor;
        private IMotionBody _body;
        private JumpController2D _jump;
        private GroundInfo _ground = GroundInfo.Airborne;
        private readonly HashSet<object> _controlOwners = new HashSet<object>();
        private bool _ready;

        /// <summary>Raised when the actor transitions from airborne to grounded.</summary>
        public event Action Landed;

        /// <summary>Raised when the actor transitions from grounded to airborne.</summary>
        public event Action LeftGround;

        /// <inheritdoc />
        public MotionState State => _state;

        /// <inheritdoc />
        public GroundInfo Ground => _ground;

        /// <summary>
        /// The tuning data driving this actor. Assigning a profile revalidates the setup, so an
        /// actor spawned without one (or given the wrong one) starts moving as soon as a valid
        /// profile arrives rather than staying inert for the rest of its life.
        /// </summary>
        public MovementProfile Profile
        {
            get => _profile;
            set
            {
                _profile = value;
                _ready = Validate();
            }
        }

        /// <summary>
        /// True while one or more owners have suspended the movement pipeline. While suspended the
        /// controller keeps grounded state up to date but does not run its steps and does not write
        /// the body, leaving velocity to whatever currently holds control.
        /// </summary>
        public bool IsPipelineSuspended => _controlOwners.Count > 0;

        /// <summary>
        /// Suspends the movement pipeline on behalf of <paramref name="owner"/> so an external
        /// system (for example a traversal ability) can own the body's velocity without the
        /// controller overwriting it. Idempotent per owner; pair each call with
        /// <see cref="ResumePipeline"/> using the same owner.
        /// </summary>
        /// <param name="owner">The object taking control. Ignored when null.</param>
        public void SuspendPipeline(object owner)
        {
            if (owner != null)
            {
                _controlOwners.Add(owner);
            }
        }

        /// <summary>
        /// Releases <paramref name="owner"/>'s suspension. The pipeline resumes only once every
        /// owner has released it.
        /// </summary>
        /// <param name="owner">The object that previously called <see cref="SuspendPipeline"/>.</param>
        public void ResumePipeline(object owner)
        {
            if (owner != null)
            {
                _controlOwners.Remove(owner);
            }
        }

        private void Awake()
        {
            _input = GetComponent<IMotionInput>();
            _groundSensor = GetComponent<IGroundSensor>();
            _body = GetComponent<IMotionBody>();
            _jump = GetComponent<JumpController2D>();
            _ready = Validate();
            BuildPipeline();
        }

        private void BuildPipeline()
        {
            _pipeline.Clear();
            _pipeline.Add(new HorizontalMovementStep());

            var crouch = GetComponent<CrouchController2D>();
            if (crouch != null)
            {
                _pipeline.Add(crouch.Step);
            }

            // Gravity runs before jumping so that a launch is the last word on vertical velocity for
            // the step. The other order costs every jump a step of gravity before the body has
            // integrated the launch at all, leaving the actor short of the configured Jump Height.
            _pipeline.Add(new GravityStep());

            if (_jump != null)
            {
                _pipeline.Add(_jump.Step);
            }

            _pipeline.Add(new SlopeProjectionStep());

            AppendProvidedSteps();
        }

        private void AppendProvidedSteps()
        {
            var providers = GetComponents<IMotionStepProvider>();
            for (int i = 0; i < providers.Length; i++)
            {
                IEnumerable<IMotionStep> steps = providers[i]?.GetMotionSteps();
                if (steps == null)
                {
                    continue;
                }

                foreach (IMotionStep step in steps)
                {
                    if (step != null)
                    {
                        _pipeline.Add(step);
                    }
                }
            }
        }

        private bool Validate()
        {
            bool ok = true;

            if (_profile == null)
            {
                Debug.LogError($"{nameof(MotionController2D)} on '{name}' requires a {nameof(MovementProfile)} to be assigned.", this);
                ok = false;
            }
            else if (_profile.Gravity <= 0f)
            {
                Debug.LogWarning($"{nameof(MotionController2D)} on '{name}': profile gravity is zero or negative; the actor will not fall.", this);
            }

            if (_input == null)
            {
                Debug.LogError($"{nameof(MotionController2D)} on '{name}' requires a component implementing {nameof(IMotionInput)} (for example InputSystemReader).", this);
                ok = false;
            }

            if (_groundSensor == null)
            {
                Debug.LogError($"{nameof(MotionController2D)} on '{name}' requires a component implementing {nameof(IGroundSensor)} (for example {nameof(GroundDetector2D)}).", this);
                ok = false;
            }

            if (_body == null)
            {
                Debug.LogError($"{nameof(MotionController2D)} on '{name}' requires a component implementing {nameof(IMotionBody)} (for example {nameof(CharacterMotor2D)}).", this);
                ok = false;
            }

            return ok;
        }

        private void FixedUpdate()
        {
            if (!_ready)
            {
                return;
            }

            _ground = _groundSensor.Sample();
            _state.WasGrounded = _state.IsGrounded;
            _state.IsGrounded = IsWalkableGround(_ground);
            _state.Velocity = _body.Velocity;

            if (!_state.WasGrounded && _state.IsGrounded)
            {
                // The body's velocity has not yet met the ground clamp this step, so this is the last
                // point at which the fall speed still exists. Record it for landing feedback.
                _state.LandingImpactSpeed = Mathf.Max(0f, -_state.Velocity.y);
            }

            if (IsPipelineSuspended)
            {
                // An external owner controls the body this step (for example a Pro traversal
                // ability). Keep the shared state an honest read-only view and keep transition
                // events live, but do not run the pipeline and do not write velocity, so there is
                // exactly one writer.
                if (_jump != null)
                {
                    // Time the player watches pass is time the jump assists spend, otherwise a
                    // coyote window or a buffered press is banked for the moment control returns.
                    _jump.AdvanceAssistTimers(Time.fixedDeltaTime);
                    _state.IsJumping = _jump.IsRising;
                }

                RaiseTransitionEvents();
                return;
            }

            var context = new MotionContext(_profile, _input, _ground, Time.fixedDeltaTime, _state);
            for (int i = 0; i < _pipeline.Count; i++)
            {
                _pipeline[i].Execute(context);
            }

            _body.Velocity = _state.Velocity;
            RaiseTransitionEvents();
        }

        private void RaiseTransitionEvents()
        {
            if (!_state.WasGrounded && _state.IsGrounded)
            {
                Landed?.Invoke();
            }
            else if (_state.WasGrounded && !_state.IsGrounded)
            {
                LeftGround?.Invoke();
            }
        }

        private bool IsWalkableGround(GroundInfo ground)
        {
            return ground.IsGrounded && ground.SlopeAngle <= _profile.MaxSlopeAngle;
        }
    }
}
