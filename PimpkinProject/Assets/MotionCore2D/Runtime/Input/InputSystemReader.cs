#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace MotionCore2D.Input
{
    /// <summary>
    /// Reads horizontal and jump intent from Unity's Input System and exposes it through
    /// <see cref="IMotionInput"/>. Jump presses are latched during <c>Update</c> so that a press
    /// occurring between fixed steps is preserved until consumed.
    /// </summary>
    /// <remarks>
    /// Assign a <c>Value</c> action of control type <c>Vector2</c> to the move action (its X axis
    /// is used) and a <c>Button</c> action to the jump action. The referenced actions are enabled
    /// while this component is enabled.
    /// </remarks>
    [AddComponentMenu("MotionCore 2D/Input/Input System Reader")]
    [DisallowMultipleComponent]
    public sealed class InputSystemReader : MonoBehaviour, IMotionInput
    {
        [SerializeField, Tooltip("Vector2 'Value' action; its X axis supplies horizontal intent.")]
        private InputActionReference _moveAction;

        [SerializeField, Tooltip("Button action that triggers a jump.")]
        private InputActionReference _jumpAction;

        [SerializeField, Tooltip("Button action held to crouch. Optional; leave unset if the actor cannot crouch.")]
        private InputActionReference _crouchAction;

        [SerializeField, Min(0f), Tooltip("How long an unconsumed jump press stays remembered, in seconds. Only matters while nothing is reading jumps (no Jump Controller 2D, or the pipeline suspended by an ability); it stops a long-forgotten press from firing a jump later. Set to 0 to never remember a press across frames.")]
        private float _jumpPressMemory = 0.2f;

        private bool _jumpPressedLatched;
        private float _jumpPressAge;

        /// <inheritdoc />
        public float Horizontal
        {
            get
            {
                InputAction action = _moveAction != null ? _moveAction.action : null;
                return action != null ? action.ReadValue<Vector2>().x : 0f;
            }
        }

        /// <inheritdoc />
        public bool JumpHeld
        {
            get
            {
                InputAction action = _jumpAction != null ? _jumpAction.action : null;
                return action != null && action.IsPressed();
            }
        }

        /// <inheritdoc />
        public bool CrouchHeld
        {
            get
            {
                InputAction action = _crouchAction != null ? _crouchAction.action : null;
                return action != null && action.IsPressed();
            }
        }

        /// <inheritdoc />
        public bool ConsumeJumpPressed()
        {
            bool pressed = _jumpPressedLatched;
            _jumpPressedLatched = false;
            _jumpPressAge = 0f;
            return pressed;
        }

        private void OnEnable()
        {
            EnableAction(_moveAction);
            EnableAction(_jumpAction);
            EnableAction(_crouchAction);
        }

        private void OnDisable()
        {
            DisableAction(_moveAction);
            DisableAction(_jumpAction);
            DisableAction(_crouchAction);

            // Drop any press taken while this reader was live; it belongs to the moment it happened,
            // not to whenever the actor is switched back on.
            _jumpPressedLatched = false;
            _jumpPressAge = 0f;
        }

        private void Update()
        {
            InputAction action = _jumpAction != null ? _jumpAction.action : null;
            if (action != null && action.WasPressedThisFrame())
            {
                _jumpPressedLatched = true;
                _jumpPressAge = 0f;
                return;
            }

            if (!_jumpPressedLatched)
            {
                return;
            }

            // In normal play the jump step consumes the latch every fixed step, so it never ages.
            // It only accumulates when nothing is reading jumps at all: an actor with no jump
            // component, or one whose pipeline an ability has suspended. Without an expiry that
            // press waits indefinitely and fires a jump the instant reading resumes, seconds later
            // and long after the player meant it. Scaled time is deliberate, so a press is not
            // forgotten while the game is paused.
            _jumpPressAge += Time.deltaTime;
            if (_jumpPressAge > _jumpPressMemory)
            {
                _jumpPressedLatched = false;
            }
        }

        private void OnValidate()
        {
            _jumpPressMemory = Mathf.Max(0f, _jumpPressMemory);
        }

        private static void EnableAction(InputActionReference reference)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.Enable();
            }
        }

        private static void DisableAction(InputActionReference reference)
        {
            if (reference != null && reference.action != null)
            {
                reference.action.Disable();
            }
        }
    }
}
#endif
