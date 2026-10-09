using UnityEngine;

namespace MotionCore2D.Input
{
    /// <summary>
    /// An <see cref="IMotionInput"/> driven entirely from script rather than from a device. Set
    /// <see cref="Horizontal"/>, <see cref="JumpHeld"/>, and <see cref="CrouchHeld"/>, and call
    /// <see cref="PressJump"/> for a one-shot jump; the actor moves exactly as if a player were at
    /// the controls.
    /// </summary>
    /// <remarks>
    /// This is the supported way to drive an actor without the Input System: enemy and NPC movement,
    /// cutscenes and scripted sequences, on-screen touch buttons, replays, network-driven remote
    /// actors, and automated tests. Unlike <c>InputSystemReader</c> it has no package or
    /// Active Input Handling requirement, so it compiles and runs in every project configuration.
    /// <para>
    /// Only one component implementing <see cref="IMotionInput"/> should be enabled on an actor, so
    /// use this instead of <c>InputSystemReader</c> rather than alongside it.
    /// </para>
    /// </remarks>
    /// <example>
    /// Walk right and jump every two seconds:
    /// <code>
    /// private ScriptedMotionInput _input;
    ///
    /// private void Awake() => _input = GetComponent&lt;ScriptedMotionInput&gt;();
    ///
    /// private void Update()
    /// {
    ///     _input.Horizontal = 1f;
    ///     if (Time.time % 2f &lt; Time.deltaTime)
    ///     {
    ///         _input.PressJump();
    ///     }
    /// }
    /// </code>
    /// </example>
    [AddComponentMenu("MotionCore 2D/Input/Scripted Motion Input")]
    [DisallowMultipleComponent]
    public sealed class ScriptedMotionInput : MonoBehaviour, IMotionInput
    {
        [SerializeField, Range(-1f, 1f), Tooltip("Horizontal intent, -1 (full left) to 1 (full right). Set this from script, or drag it here while testing in Play mode.")]
        private float _horizontal;

        [SerializeField, Tooltip("Whether the jump control counts as held. Holding it produces a full-height jump; releasing it early cuts the jump short.")]
        private bool _jumpHeld;

        [SerializeField, Tooltip("Whether the crouch control counts as held. Only has an effect when the actor has a Crouch Controller 2D.")]
        private bool _crouchHeld;

        [SerializeField, Min(0f), Tooltip("How long a queued jump press stays remembered if nothing reads it, in seconds. Mirrors Input System Reader so a forgotten press cannot fire a jump much later. Set to 0 to never remember a press across frames.")]
        private float _jumpPressMemory = 0.2f;

        private bool _jumpPressQueued;
        private float _jumpPressAge;

        /// <inheritdoc />
        public float Horizontal
        {
            get => Mathf.Clamp(_horizontal, -1f, 1f);
            set => _horizontal = Mathf.Clamp(value, -1f, 1f);
        }

        /// <inheritdoc />
        public bool JumpHeld
        {
            get => _jumpHeld;
            set => _jumpHeld = value;
        }

        /// <inheritdoc />
        public bool CrouchHeld
        {
            get => _crouchHeld;
            set => _crouchHeld = value;
        }

        /// <summary>
        /// Queues a single jump press, as though the control had just been pushed down. The next
        /// simulation step consumes it. Queueing again before it is consumed has no extra effect.
        /// </summary>
        /// <remarks>
        /// This is edge-triggered on purpose: it is the press that starts a jump, while
        /// <see cref="JumpHeld"/> is what decides how high the jump goes. For a full-height jump set
        /// <see cref="JumpHeld"/> to <c>true</c> and keep it there until the actor is falling; for a
        /// short hop, press and leave <see cref="JumpHeld"/> <c>false</c>.
        /// </remarks>
        public void PressJump()
        {
            _jumpPressQueued = true;
            _jumpPressAge = 0f;
        }

        /// <summary>
        /// Convenience for a complete one-shot jump input: queues a press and sets
        /// <see cref="JumpHeld"/>, so the actor performs a full-height jump.
        /// </summary>
        /// <param name="held">Whether to hold the control. Pass <c>false</c> for a short hop.</param>
        public void Jump(bool held = true)
        {
            PressJump();
            _jumpHeld = held;
        }

        /// <summary>
        /// Clears all intent: no horizontal movement, nothing held, and no queued press. Useful when
        /// handing control back after a cutscene, or when an actor dies or is disabled, so stale
        /// intent cannot drive it on the next step.
        /// </summary>
        public void Clear()
        {
            _horizontal = 0f;
            _jumpHeld = false;
            _crouchHeld = false;
            _jumpPressQueued = false;
            _jumpPressAge = 0f;
        }

        /// <inheritdoc />
        public bool ConsumeJumpPressed()
        {
            bool pressed = _jumpPressQueued;
            _jumpPressQueued = false;
            _jumpPressAge = 0f;
            return pressed;
        }

        private void Update()
        {
            if (!_jumpPressQueued)
            {
                return;
            }

            // A queued press is normally consumed on the next fixed step. It only ages when nothing
            // reads jumps at all - an actor with no Jump Controller 2D, or one whose pipeline an
            // ability has suspended - and without an expiry it would wait indefinitely and fire long
            // after the script meant it. Scaled time, so a press survives a pause.
            _jumpPressAge += Time.deltaTime;
            if (_jumpPressAge > _jumpPressMemory)
            {
                _jumpPressQueued = false;
            }
        }

        private void OnDisable()
        {
            _jumpPressQueued = false;
            _jumpPressAge = 0f;
        }

        private void OnValidate()
        {
            _horizontal = Mathf.Clamp(_horizontal, -1f, 1f);
            _jumpPressMemory = Mathf.Max(0f, _jumpPressMemory);
        }
    }
}
