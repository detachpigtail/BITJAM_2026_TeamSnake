using UnityEngine;

namespace MotionCore2D.Configuration
{
    /// <summary>
    /// Serialized, validated tuning data for a 2D motion actor. This is a pure data asset: it
    /// holds the parameters that movement logic will consume, but contains no behaviour itself.
    /// </summary>
    /// <remarks>
    /// A profile is configuration only and is treated as read-only at runtime; per-actor mutable
    /// state lives in <see cref="MotionState"/>. Many actors may safely share a single profile by
    /// reference. Field names and units are kept stable to preserve serialized data across
    /// versions.
    /// </remarks>
    [CreateAssetMenu(
        fileName = "MovementProfile",
        menuName = "MotionCore 2D/Movement Profile",
        order = 0)]
    public sealed class MovementProfile : ScriptableObject
    {
        [Header("Horizontal")]
        [SerializeField, Min(0f), Tooltip("Maximum horizontal speed, in world units per second.")]
        private float _maxSpeed = 8f;

        [SerializeField, Min(0f), Tooltip("Horizontal acceleration toward the target speed, in units per second squared.")]
        private float _acceleration = 60f;

        [SerializeField, Min(0f), Tooltip("Horizontal deceleration when input opposes motion or is released, in units per second squared.")]
        private float _deceleration = 60f;

        [SerializeField, Range(0f, 1f), Tooltip("Fraction of horizontal control retained while airborne (0 = none, 1 = full).")]
        private float _airControlFactor = 0.6f;

        [Header("Crouch")]
        [SerializeField, Range(0f, 1f), Tooltip("Top horizontal speed while crouched, as a fraction of Max Speed (0 = cannot move while crouched, 1 = no slowdown).")]
        private float _crouchSpeedMultiplier = 0.5f;

        [SerializeField, Range(0.1f, 1f), Tooltip("Crouched collider height as a fraction of the standing height. The feet stay planted; the top is lowered.")]
        private float _crouchHeightFactor = 0.5f;

        [Header("Jump")]
        [SerializeField, Min(0f), Tooltip("Apex height of a full jump, in world units.")]
        private float _jumpHeight = 3.5f;

        [SerializeField, Range(0f, 1f), Tooltip("Fraction of upward velocity retained when the jump button is released early (variable jump height).")]
        private float _variableJumpCutMultiplier = 0.5f;

        [Header("Gravity")]
        [SerializeField, Min(0f), Tooltip("Downward acceleration while rising, in units per second squared.")]
        private float _gravity = 40f;

        [SerializeField, Min(1f), Tooltip("Multiplier applied to gravity while falling, for a snappier descent.")]
        private float _fallGravityMultiplier = 1.6f;

        [SerializeField, Min(0f), Tooltip("Maximum downward speed, in world units per second.")]
        private float _maxFallSpeed = 25f;

        [Header("Assist")]
        [SerializeField, Min(0f), Tooltip("Grace period after leaving ground during which a jump is still permitted (coyote time), in seconds.")]
        private float _coyoteTime = 0.1f;

        [SerializeField, Min(0f), Tooltip("Window before landing during which a jump press is remembered and applied on contact (jump buffering), in seconds.")]
        private float _jumpBufferTime = 0.12f;

        [Header("Slope")]
        [SerializeField, Range(0f, 89f), Tooltip("Steepest slope angle, in degrees, the actor treats as walkable ground. This is a movement policy; ground detection reports the raw angle and leaves this judgement to the consumer.")]
        private float _maxSlopeAngle = 50f;

        /// <summary>Maximum horizontal speed, in world units per second.</summary>
        public float MaxSpeed => _maxSpeed;

        /// <summary>Horizontal acceleration toward the target speed, in units per second squared.</summary>
        public float Acceleration => _acceleration;

        /// <summary>Horizontal deceleration when input opposes motion or is released, in units per second squared.</summary>
        public float Deceleration => _deceleration;

        /// <summary>Fraction of horizontal control retained while airborne, in the range 0 to 1.</summary>
        public float AirControlFactor => _airControlFactor;

        /// <summary>Top horizontal speed while crouched, as a fraction of <see cref="MaxSpeed"/>, in the range 0 to 1.</summary>
        public float CrouchSpeedMultiplier => _crouchSpeedMultiplier;

        /// <summary>Crouched collider height as a fraction of the standing height, in the range 0.1 to 1.</summary>
        public float CrouchHeightFactor => _crouchHeightFactor;

        /// <summary>Apex height of a full jump, in world units.</summary>
        public float JumpHeight => _jumpHeight;

        /// <summary>Fraction of upward velocity retained when the jump button is released early, in the range 0 to 1.</summary>
        public float VariableJumpCutMultiplier => _variableJumpCutMultiplier;

        /// <summary>Downward acceleration while rising, in units per second squared.</summary>
        public float Gravity => _gravity;

        /// <summary>Multiplier applied to gravity while falling. Always at least 1.</summary>
        public float FallGravityMultiplier => _fallGravityMultiplier;

        /// <summary>Maximum downward speed, in world units per second.</summary>
        public float MaxFallSpeed => _maxFallSpeed;

        /// <summary>Grace period after leaving ground during which a jump is still permitted, in seconds.</summary>
        public float CoyoteTime => _coyoteTime;

        /// <summary>Window before landing during which a jump press is remembered, in seconds.</summary>
        public float JumpBufferTime => _jumpBufferTime;

        /// <summary>Steepest slope angle, in degrees, the actor treats as walkable ground.</summary>
        public float MaxSlopeAngle => _maxSlopeAngle;

        /// <summary>
        /// Clamps all parameters into their supported ranges. Called automatically by the editor
        /// whenever a value changes, and may be called manually after programmatic edits.
        /// </summary>
        public void Validate()
        {
            _maxSpeed = Mathf.Max(0f, _maxSpeed);
            _acceleration = Mathf.Max(0f, _acceleration);
            _deceleration = Mathf.Max(0f, _deceleration);
            _airControlFactor = Mathf.Clamp01(_airControlFactor);
            _crouchSpeedMultiplier = Mathf.Clamp01(_crouchSpeedMultiplier);
            _crouchHeightFactor = Mathf.Clamp(_crouchHeightFactor, 0.1f, 1f);
            _jumpHeight = Mathf.Max(0f, _jumpHeight);
            _variableJumpCutMultiplier = Mathf.Clamp01(_variableJumpCutMultiplier);
            _gravity = Mathf.Max(0f, _gravity);
            _fallGravityMultiplier = Mathf.Max(1f, _fallGravityMultiplier);
            _maxFallSpeed = Mathf.Max(0f, _maxFallSpeed);
            _coyoteTime = Mathf.Max(0f, _coyoteTime);
            _jumpBufferTime = Mathf.Max(0f, _jumpBufferTime);
            _maxSlopeAngle = Mathf.Clamp(_maxSlopeAngle, 0f, 89f);
        }

        private void OnValidate()
        {
            Validate();
        }
    }
}
