using System;

namespace MotionCore2D.Jump
{
    /// <summary>
    /// Pure, engine-independent decision logic for jumping. Given the per-step situation it tracks
    /// coyote time and jump buffering, decides when a jump begins, computes the launch velocity for
    /// the configured height, and applies variable jump height when the control is released early.
    /// </summary>
    /// <remarks>
    /// The machine owns only the small amount of state these features require (the countdown timers
    /// and the progress of an upward jump). It performs no input reading, no physics, and no
    /// velocity application; it consumes <see cref="Inputs"/> and returns a <see cref="Result"/> for
    /// the caller to apply. This keeps the logic fully unit testable.
    /// </remarks>
    public sealed class JumpStateMachine
    {
        private const float MinGravity = 0.0001f;

        /// <summary>
        /// Upper bound, in seconds, on how long a launch may keep ground readings suppressed. The
        /// suppression normally ends on its own as soon as the actor stops rising; this cap is a
        /// safety net for the case where something else (a lift, a rising platform) keeps the actor
        /// moving upward indefinitely, so an actor can never be locked out of jumping.
        /// </summary>
        private const float LaunchGroundSuppressionCap = 0.25f;

        private float _coyoteRemaining;
        private float _bufferRemaining;
        private float _suppressGroundRemaining;
        private bool _ascending;

        /// <summary>
        /// Whether a jump launched by this machine is still rising. Becomes <c>false</c> at the
        /// apex, when the jump is cut short, and on landing.
        /// </summary>
        public bool IsRising => _ascending;

        /// <summary>
        /// Advances the timers and evaluates jump decisions for a single simulation step.
        /// </summary>
        /// <param name="inputs">The situation for this step.</param>
        /// <returns>The vertical-velocity change to apply, if any.</returns>
        public Result Evaluate(in Inputs inputs)
        {
            float deltaTime = Math.Max(0f, inputs.DeltaTime);

            // A launch does not move the actor until the body integrates at the end of the step, so
            // for the next step or two the ground probe still finds the surface the actor just left.
            // Taking those readings at face value would re-arm coyote time mid-ascent (handing out a
            // free second jump) and forget that a jump is in progress (silently disabling variable
            // jump height). Ignore ground while the launch is still carrying the actor upward.
            _suppressGroundRemaining = Math.Max(0f, _suppressGroundRemaining - deltaTime);
            bool suppressGround = _suppressGroundRemaining > 0f && inputs.VelocityY > 0f;
            bool grounded = inputs.IsGrounded && !suppressGround;

            if (grounded)
            {
                _coyoteRemaining = inputs.CoyoteTime;
                _suppressGroundRemaining = 0f;
                _ascending = false;
            }
            else
            {
                _coyoteRemaining = Math.Max(0f, _coyoteRemaining - deltaTime);
            }

            if (inputs.JumpPressed)
            {
                _bufferRemaining = inputs.JumpBufferTime;
            }
            else
            {
                _bufferRemaining = Math.Max(0f, _bufferRemaining - deltaTime);
            }

            // Both windows are assists that widen an existing opportunity; neither is a precondition
            // for one. A press this step counts even when the buffer window is zero, and standing on
            // the ground counts even when the coyote window is zero, so tuning either assist down to
            // zero sharpens the feel instead of disabling jumping altogether.
            bool wantsJump = inputs.JumpPressed || _bufferRemaining > 0f;
            bool canJump = grounded || _coyoteRemaining > 0f;

            if (wantsJump && canJump)
            {
                _bufferRemaining = 0f;
                _coyoteRemaining = 0f;
                _ascending = true;
                _suppressGroundRemaining = LaunchGroundSuppressionCap;
                return Result.Launch(LaunchVelocity(inputs, deltaTime));
            }

            if (_ascending)
            {
                if (inputs.VelocityY <= 0f)
                {
                    _ascending = false;
                }
                else if (!inputs.JumpHeld)
                {
                    _ascending = false;
                    float multiplier = Clamp01(inputs.CutMultiplier);
                    return Result.Cut(inputs.VelocityY * multiplier);
                }
            }

            return Result.None;
        }

        /// <summary>
        /// Expires the assist windows for <paramref name="deltaTime"/> seconds without evaluating a
        /// jump. Call it for any step the machine is skipped — while something else owns the body,
        /// for instance — so that time the player can see passing is time the assists spend.
        /// </summary>
        /// <remarks>
        /// Without this, a coyote window or a buffered press frozen at the moment control was taken
        /// away is still waiting to be spent when control returns, and the actor gets a free jump out
        /// of a ledge it left seconds ago. An ascent already under way is deliberately preserved, so
        /// a jump interrupted for a step still honours an early release afterwards.
        /// </remarks>
        /// <param name="deltaTime">Elapsed time to charge against the windows, in seconds.</param>
        public void AdvanceAssistTimers(float deltaTime)
        {
            float dt = Math.Max(0f, deltaTime);
            _coyoteRemaining = Math.Max(0f, _coyoteRemaining - dt);
            _bufferRemaining = Math.Max(0f, _bufferRemaining - dt);
            _suppressGroundRemaining = Math.Max(0f, _suppressGroundRemaining - dt);
        }

        /// <summary>
        /// Clears all jump state, returning the machine to the situation of an actor that has never
        /// jumped. Call it when an actor is respawned, teleported, or re-enabled so that stale
        /// timers and a stale ascent cannot affect the next step.
        /// </summary>
        public void Reset()
        {
            _coyoteRemaining = 0f;
            _bufferRemaining = 0f;
            _suppressGroundRemaining = 0f;
            _ascending = false;
        }

        /// <summary>
        /// The upward velocity that reaches <see cref="Inputs.JumpHeight"/> once the caller
        /// integrates it.
        /// </summary>
        /// <remarks>
        /// The closed form <c>sqrt(2 g h)</c> is exact for continuous motion, but the caller advances
        /// the body with semi-implicit Euler: the launch velocity is applied for a whole step before
        /// gravity first reduces it, which overshoots the target apex by half a step of travel.
        /// Subtracting that half step up front brings the integrated apex to within
        /// <c>g * dt * dt / 8</c> of the configured height, under two millimetres at typical
        /// settings, so Jump Height means what the inspector says it means.
        /// </remarks>
        private static float LaunchVelocity(in Inputs inputs, float deltaTime)
        {
            float gravity = Math.Max(MinGravity, inputs.Gravity);
            float height = Math.Max(0f, inputs.JumpHeight);
            float ideal = (float)Math.Sqrt(2.0 * gravity * height);
            float halfStep = gravity * deltaTime * 0.5f;
            return Math.Max(0f, ideal - halfStep);
        }

        private static float Clamp01(float value) =>
            value < 0f ? 0f : (value > 1f ? 1f : value);

        /// <summary>
        /// Immutable description of the situation for a single jump evaluation.
        /// </summary>
        public readonly struct Inputs
        {
            /// <summary>Simulation time step, in seconds.</summary>
            public float DeltaTime { get; }

            /// <summary>Whether the actor is grounded this step.</summary>
            public bool IsGrounded { get; }

            /// <summary>Whether a jump was pressed since the previous step.</summary>
            public bool JumpPressed { get; }

            /// <summary>Whether the jump control is currently held.</summary>
            public bool JumpHeld { get; }

            /// <summary>Current vertical velocity, in world units per second.</summary>
            public float VelocityY { get; }

            /// <summary>Downward gravitational acceleration magnitude, in units per second squared.</summary>
            public float Gravity { get; }

            /// <summary>Coyote-time window, in seconds.</summary>
            public float CoyoteTime { get; }

            /// <summary>Jump-buffer window, in seconds.</summary>
            public float JumpBufferTime { get; }

            /// <summary>Target apex height of a full jump, in world units.</summary>
            public float JumpHeight { get; }

            /// <summary>Fraction of upward velocity retained on early release, in the range 0 to 1.</summary>
            public float CutMultiplier { get; }

            /// <summary>Initialises a new <see cref="Inputs"/>.</summary>
            /// <param name="deltaTime">Simulation time step, in seconds.</param>
            /// <param name="isGrounded">Whether the actor is grounded this step.</param>
            /// <param name="jumpPressed">Whether a jump was pressed since the previous step.</param>
            /// <param name="jumpHeld">Whether the jump control is currently held.</param>
            /// <param name="velocityY">Current vertical velocity, in world units per second.</param>
            /// <param name="gravity">Downward gravitational acceleration magnitude.</param>
            /// <param name="coyoteTime">Coyote-time window, in seconds.</param>
            /// <param name="jumpBufferTime">Jump-buffer window, in seconds.</param>
            /// <param name="jumpHeight">Target apex height of a full jump, in world units.</param>
            /// <param name="cutMultiplier">Fraction of upward velocity retained on early release.</param>
            public Inputs(
                float deltaTime,
                bool isGrounded,
                bool jumpPressed,
                bool jumpHeld,
                float velocityY,
                float gravity,
                float coyoteTime,
                float jumpBufferTime,
                float jumpHeight,
                float cutMultiplier)
            {
                DeltaTime = deltaTime;
                IsGrounded = isGrounded;
                JumpPressed = jumpPressed;
                JumpHeld = jumpHeld;
                VelocityY = velocityY;
                Gravity = gravity;
                CoyoteTime = coyoteTime;
                JumpBufferTime = jumpBufferTime;
                JumpHeight = jumpHeight;
                CutMultiplier = cutMultiplier;
            }
        }

        /// <summary>
        /// Immutable outcome of a jump evaluation: whether vertical velocity should change, to what
        /// value, and whether the change begins a new jump.
        /// </summary>
        public readonly struct Result
        {
            private Result(bool changesVerticalVelocity, float verticalVelocity, bool launched)
            {
                ChangesVerticalVelocity = changesVerticalVelocity;
                VerticalVelocity = verticalVelocity;
                Launched = launched;
            }

            /// <summary>Whether the caller should overwrite the vertical velocity this step.</summary>
            public bool ChangesVerticalVelocity { get; }

            /// <summary>The vertical velocity to apply when <see cref="ChangesVerticalVelocity"/> is <c>true</c>.</summary>
            public float VerticalVelocity { get; }

            /// <summary>
            /// Whether this result begins a new jump rather than cutting one short. Lets a caller
            /// raise a jump notification without having to infer it from the velocity.
            /// </summary>
            public bool Launched { get; }

            /// <summary>A result that changes nothing.</summary>
            public static Result None => new Result(false, 0f, false);

            /// <summary>Creates a result that begins a jump at <paramref name="verticalVelocity"/>.</summary>
            /// <param name="verticalVelocity">The launch velocity to apply.</param>
            /// <returns>A change result that begins a new jump.</returns>
            public static Result Launch(float verticalVelocity) =>
                new Result(true, verticalVelocity, true);

            /// <summary>Creates a result that cuts an in-progress jump to <paramref name="verticalVelocity"/>.</summary>
            /// <param name="verticalVelocity">The reduced vertical velocity to apply.</param>
            /// <returns>A change result that does not begin a new jump.</returns>
            public static Result Cut(float verticalVelocity) =>
                new Result(true, verticalVelocity, false);

            /// <summary>Creates a result that sets vertical velocity to <paramref name="verticalVelocity"/>.</summary>
            /// <param name="verticalVelocity">The vertical velocity to apply.</param>
            /// <returns>A change result that begins a new jump.</returns>
            [Obsolete("Use Launch for a new jump or Cut for an early release. This alias behaves as Launch and will be removed in 2.0.")]
            public static Result SetVerticalVelocity(float verticalVelocity) =>
                Launch(verticalVelocity);
        }
    }
}
